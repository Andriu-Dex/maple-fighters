using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.Services;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Presenter para la vista de login de jugadores v2.
    /// Coordina el flujo: Email -> (Existente: Password) / (Nuevo: Selección -> Registro).
    /// Sigue el patrón MVP (Model-View-Presenter).
    /// </summary>
    public class PlayerLoginPresenter : IDisposable
    {
        private readonly IPlayerLoginView view;
        private readonly IPlayerLoginApi loginApi;
        private readonly ICredentialValidator validator;

        private PlayerLoginState currentState;
        private string currentEmail;
        private string currentPlayerName;
        private string selectedCharacterClass;
        private IPlayerCredentials currentPlayerData;

        #region Events

        /// <summary>
        /// Se dispara cuando el login es exitoso.
        /// Parámetros: (email, nombre del jugador, datos del jugador).
        /// </summary>
        public event Action<string, string, IPlayerCredentials> LoginSuccessful;

        /// <summary>
        /// Se dispara cuando el usuario quiere volver atrás.
        /// </summary>
        public event Action BackRequested;

        #endregion

        #region Properties

        public PlayerLoginState CurrentState => currentState;
        public string CurrentEmail => currentEmail;
        public string CurrentPlayerName => currentPlayerName;
        public IPlayerCredentials CurrentPlayerData => currentPlayerData;

        #endregion

        public PlayerLoginPresenter(
            IPlayerLoginView view,
            IPlayerLoginApi loginApi,
            ICredentialValidator validator)
        {
            this.view = view;
            this.loginApi = loginApi;
            this.validator = validator;

            if (view == null) Debug.LogWarning("[PlayerLoginPresenter] view es null");
            if (loginApi == null) Debug.LogWarning("[PlayerLoginPresenter] loginApi es null");
            if (validator == null) Debug.LogWarning("[PlayerLoginPresenter] validator es null");

            SubscribeToEvents();
            SetState(PlayerLoginState.EnterEmail);
        }

        public void Dispose()
        {
            UnsubscribeFromEvents();
        }

        #region Event Subscriptions

        private void SubscribeToEvents()
        {
            if (view != null)
            {
                view.ConfirmButtonClicked += OnConfirmButtonClicked;
                view.BackButtonClicked += OnBackButtonClicked;
                view.InputFieldChanged += OnInputFieldChanged;
                view.CharacterClassSelected += OnCharacterClassSelected;
                view.RegistrationConfirmed += OnRegistrationConfirmed;
            }

            if (loginApi != null)
            {
                // v2 callbacks
                loginApi.CheckEmailCallback += OnCheckEmailCallback;
                loginApi.LoginByEmailCallback += OnLoginByEmailCallback;
                loginApi.RegisterWithEmailCallback += OnRegisterWithEmailCallback;

                // v1 callbacks (legacy)
                loginApi.CheckPlayerExistsCallback += OnCheckPlayerExistsCallback;
                loginApi.LoginCallback += OnLoginCallback;
            }
        }

        private void UnsubscribeFromEvents()
        {
            if (view != null)
            {
                view.ConfirmButtonClicked -= OnConfirmButtonClicked;
                view.BackButtonClicked -= OnBackButtonClicked;
                view.InputFieldChanged -= OnInputFieldChanged;
                view.CharacterClassSelected -= OnCharacterClassSelected;
                view.RegistrationConfirmed -= OnRegistrationConfirmed;
            }

            if (loginApi != null)
            {
                loginApi.CheckEmailCallback -= OnCheckEmailCallback;
                loginApi.LoginByEmailCallback -= OnLoginByEmailCallback;
                loginApi.RegisterWithEmailCallback -= OnRegisterWithEmailCallback;
                loginApi.CheckPlayerExistsCallback -= OnCheckPlayerExistsCallback;
                loginApi.LoginCallback -= OnLoginCallback;
            }
        }

        #endregion

        #region State Management

        private void SetState(PlayerLoginState newState)
        {
            currentState = newState;
            UpdateViewForState();
            Debug.Log($"[PlayerLoginPresenter] Estado: {newState}");
        }

        private void UpdateViewForState()
        {
            if (view == null) return;

            switch (currentState)
            {
                // v2 States
                case PlayerLoginState.EnterEmail:
                    view.ShowLoginPanel();
                    view.HideCharacterSelectionPanel();
                    view.HideRegistrationPanel();
                    view.Title = "Ingresa tu email";
                    view.Placeholder = "email@ejemplo.com";
                    view.StatusMessage = string.Empty;
                    view.SetPasswordMode(false);
                    view.ClearInput();
                    view.EnableInteraction();
                    view.DisableConfirmButton();
                    view.FocusInput();
                    break;

                case PlayerLoginState.CheckingEmail:
                    view.StatusMessage = "Verificando email...";
                    view.DisableInteraction();
                    break;

                case PlayerLoginState.EnterPasswordForLogin:
                    view.ShowLoginPanel();
                    view.HideCharacterSelectionPanel();
                    view.HideRegistrationPanel();
                    view.Title = $"Bienvenido de nuevo";
                    view.Placeholder = "Contraseña";
                    view.StatusMessage = $"Email: {currentEmail}";
                    view.SetPasswordMode(true);
                    view.ClearInput();
                    view.EnableInteraction();
                    view.DisableConfirmButton();
                    view.FocusInput();
                    break;

                case PlayerLoginState.EnterPasswordForNewUser:
                    view.ShowLoginPanel();
                    view.HideCharacterSelectionPanel();
                    view.HideRegistrationPanel();
                    view.Title = "Crear contraseña";
                    view.Placeholder = "Nueva contraseña";
                    view.StatusMessage = $"Email: {currentEmail}\nCrea una contraseña para tu cuenta";
                    view.SetPasswordMode(true);
                    view.ClearInput();
                    view.EnableInteraction();
                    view.DisableConfirmButton();
                    view.FocusInput();
                    break;

                case PlayerLoginState.SelectCharacter:
                    view.HideLoginPanel();
                    view.HideRegistrationPanel();
                    view.CurrentEmail = currentEmail;
                    view.ShowCharacterSelectionPanel();
                    break;

                case PlayerLoginState.EnterRegistrationData:
                    view.HideLoginPanel();
                    view.HideCharacterSelectionPanel();
                    view.SelectedCharacterClass = selectedCharacterClass;
                    view.ShowRegistrationPanel();
                    break;

                case PlayerLoginState.Validating:
                    view.StatusMessage = "Validando...";
                    view.DisableInteraction();
                    break;

                case PlayerLoginState.Success:
                    view.StatusMessage = "¡Login exitoso!";
                    view.DisableInteraction();
                    break;

                case PlayerLoginState.Blocked:
                    view.StatusMessage = "⚠️ Cuenta bloqueada por seguridad.\nContacta al usuario 'Admin' para desbloquearla.";
                    view.DisableConfirmButton();
                    view.EnableBackButton(); // Habilitar explícitamente para poder volver e intentar con otra cuenta
                    break;

                case PlayerLoginState.Error:
                    view.EnableInteraction();
                    break;

                // v1 Legacy States
                case PlayerLoginState.EnterName:
                    view.Title = "Ingresa tu nombre";
                    view.Placeholder = "Nombre de jugador";
                    view.StatusMessage = string.Empty;
                    view.SetPasswordMode(false);
                    view.ClearInput();
                    view.EnableInteraction();
                    view.DisableConfirmButton();
                    view.FocusInput();
                    break;

                case PlayerLoginState.CheckingName:
                    view.StatusMessage = "Verificando...";
                    view.DisableInteraction();
                    break;

                case PlayerLoginState.EnterPassword:
                    view.Title = $"Bienvenido, {currentPlayerName}";
                    view.Placeholder = "Contraseña";
                    view.StatusMessage = "Ingresa tu contraseña";
                    view.SetPasswordMode(true);
                    view.ClearInput();
                    view.EnableInteraction();
                    view.DisableConfirmButton();
                    view.FocusInput();
                    break;

                case PlayerLoginState.NewPlayer:
                    view.StatusMessage = "Creando nuevo personaje...";
                    view.DisableInteraction();
                    break;
            }
        }

        #endregion

        #region View Event Handlers

        private void OnConfirmButtonClicked(string input)
        {
            switch (currentState)
            {
                case PlayerLoginState.EnterEmail:
                    HandleEmailSubmitted(input);
                    break;

                case PlayerLoginState.EnterPasswordForLogin:
                    HandlePasswordForLoginSubmitted(input);
                    break;

                case PlayerLoginState.EnterPasswordForNewUser:
                    HandlePasswordForNewUserSubmitted(input);
                    break;

                // Legacy v1
                case PlayerLoginState.EnterName:
                    HandleNameSubmitted(input);
                    break;

                case PlayerLoginState.EnterPassword:
                    HandlePasswordSubmitted(input);
                    break;
            }
        }

        private void OnBackButtonClicked()
        {
            switch (currentState)
            {
                case PlayerLoginState.EnterPasswordForLogin:
                    currentEmail = null;
                    SetState(PlayerLoginState.EnterEmail);
                    break;

                case PlayerLoginState.EnterPasswordForNewUser:
                    currentEmail = null;
                    SetState(PlayerLoginState.EnterEmail);
                    break;

                case PlayerLoginState.SelectCharacter:
                    currentEmail = null;
                    SetState(PlayerLoginState.EnterEmail);
                    break;

                case PlayerLoginState.EnterRegistrationData:
                    SetState(PlayerLoginState.SelectCharacter);
                    break;

                // Legacy v1
                case PlayerLoginState.EnterPassword:
                    currentPlayerName = null;
                    SetState(PlayerLoginState.EnterName);
                    break;

                default:
                    BackRequested?.Invoke();
                    break;
            }
        }

        private void OnInputFieldChanged(string text)
        {
            if (view == null) return;

            bool isValid = false;

            switch (currentState)
            {
                case PlayerLoginState.EnterEmail:
                    isValid = validator?.IsValidEmail(text) ?? text.Contains("@");
                    break;

                case PlayerLoginState.EnterPasswordForLogin:
                case PlayerLoginState.EnterPasswordForNewUser:
                case PlayerLoginState.EnterPassword:
                    isValid = validator?.IsValidPasswordFormat(text) ?? !string.IsNullOrEmpty(text);
                    break;

                case PlayerLoginState.EnterName:
                    isValid = validator?.IsValidPlayerName(text) ?? !string.IsNullOrEmpty(text);
                    break;
            }

            if (isValid)
                view.EnableConfirmButton();
            else
                view.DisableConfirmButton();
        }

        private void OnCharacterClassSelected(string characterClass)
        {
            selectedCharacterClass = characterClass;
            Debug.Log($"[PlayerLoginPresenter] Personaje seleccionado: {characterClass}");
        }

        private void OnRegistrationConfirmed(string playerName, string password)
        {
            HandleRegistrationConfirmed(playerName, password);
        }

        #endregion

        #region v2 Email Flow Handlers

        private void HandleEmailSubmitted(string email)
        {
            if (validator != null && !validator.IsValidEmail(email))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidEmailFormat);
                return;
            }

            currentEmail = email.Trim().ToLowerInvariant();
            SetState(PlayerLoginState.CheckingEmail);

            if (loginApi != null)
            {
                loginApi.CheckEmail(currentEmail);
            }
            else
            {
                Debug.LogError("[PlayerLoginPresenter] loginApi es null");
                view.StatusMessage = "Error: API no disponible";
                SetState(PlayerLoginState.Error);
            }
        }

        private void HandlePasswordForLoginSubmitted(string password)
        {
            if (validator != null && !validator.IsValidPasswordFormat(password))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPasswordFormat);
                return;
            }

            SetState(PlayerLoginState.Validating);

            if (loginApi != null)
            {
                loginApi.LoginByEmail(currentEmail, password);
            }
            else
            {
                Debug.LogError("[PlayerLoginPresenter] loginApi es null");
                view.StatusMessage = "Error: API no disponible";
                SetState(PlayerLoginState.Error);
            }
        }

        private void HandlePasswordForNewUserSubmitted(string password)
        {
            if (validator != null && !validator.IsValidPasswordFormat(password))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPasswordFormat);
                return;
            }

            SetState(PlayerLoginState.Validating);

            if (loginApi != null)
            {
                // Crear cuenta con email y contraseña
                loginApi.RegisterWithEmail(currentEmail, password);
            }
            else
            {
                Debug.LogError("[PlayerLoginPresenter] loginApi es null");
                view.StatusMessage = "Error: API no disponible";
                SetState(PlayerLoginState.Error);
            }
        }

        private void HandleRegistrationConfirmed(string playerName, string password)
        {
            // Validar nombre
            if (validator != null && !validator.IsValidPlayerName(playerName))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPlayerName);
                return;
            }

            // Validar contraseña
            if (validator != null && !validator.IsValidPasswordFormat(password))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPasswordFormat);
                return;
            }

            currentPlayerName = playerName.Trim();
            SetState(PlayerLoginState.Validating);

            if (loginApi != null)
            {
                // Primero crear cuenta si no existe, luego completar registro
                loginApi.CompleteRegistration(currentEmail, currentPlayerName, password, selectedCharacterClass);
            }
            else
            {
                Debug.LogError("[PlayerLoginPresenter] loginApi es null");
                view.StatusMessage = "Error: API no disponible";
                SetState(PlayerLoginState.Error);
            }
        }

        #endregion

        #region v2 API Callbacks

        private void OnCheckEmailCallback(EmailCheckResult result, string email, IPlayerCredentials playerData)
        {
            if (email != currentEmail) return;

            switch (result)
            {
                case EmailCheckResult.ExistsComplete:
                    // Usuario existente con registro completo -> pedir contraseña
                    currentPlayerData = playerData;
                    currentPlayerName = playerData?.PlayerName;
                    if (loginApi.IsEmailBlocked(email))
                    {
                        SetState(PlayerLoginState.Blocked);
                    }
                    else
                    {
                        SetState(PlayerLoginState.EnterPasswordForLogin);
                    }
                    break;

                case EmailCheckResult.ExistsIncomplete:
                    // Usuario existe pero sin contraseña -> pedir que cree contraseña
                    currentPlayerData = playerData;
                    SetState(PlayerLoginState.EnterPasswordForNewUser);
                    break;

                case EmailCheckResult.NotExists:
                    // Usuario nuevo -> pedir que cree contraseña
                    SetState(PlayerLoginState.EnterPasswordForNewUser);
                    break;

                case EmailCheckResult.InvalidFormat:
                    view.StatusMessage = "Formato de email inválido";
                    SetState(PlayerLoginState.EnterEmail);
                    break;

                default:
                    view.StatusMessage = "Error al verificar email";
                    SetState(PlayerLoginState.Error);
                    break;
            }
        }

        private void OnLoginByEmailCallback(LoginResult result, int remainingAttempts, string message, IPlayerCredentials playerData)
        {
            // Solo procesar si este Presenter está esperando un callback de login
            // Esto evita que múltiples instancias de Presenter procesen el mismo callback
            if (currentState != PlayerLoginState.Validating)
            {
                Debug.Log($"[PlayerLoginPresenter] Ignorando callback de login - estado actual: {currentState}");
                return;
            }
            
            Debug.Log($"[PlayerLoginPresenter] OnLoginByEmailCallback: result={result}, message={message}");
            
            switch (result)
            {
                case LoginResult.Success:
                    Debug.Log($"[PlayerLoginPresenter] Login exitoso, disparando LoginSuccessful para {currentEmail}");
                    currentPlayerData = playerData;
                    currentPlayerName = playerData?.PlayerName;
                    SetState(PlayerLoginState.Success);
                    LoginSuccessful?.Invoke(currentEmail, currentPlayerName, currentPlayerData);
                    break;

                case LoginResult.WrongPassword:
                    Debug.Log($"[PlayerLoginPresenter] Contraseña incorrecta, intentos restantes: {remainingAttempts}");
                    SetState(PlayerLoginState.EnterPasswordForLogin);
                    view.StatusMessage = message;
                    view.ClearInput();
                    view.FocusInput();
                    break;

                case LoginResult.Blocked:
                    SetState(PlayerLoginState.Blocked);
                    view.StatusMessage = message;
                    break;

                case LoginResult.RegistrationIncomplete:
                    // Redirigir a CharacterViewController - login exitoso
                    Debug.Log($"[PlayerLoginPresenter] Registro incompleto, disparando LoginSuccessful para {currentEmail}");
                    currentPlayerData = playerData;
                    SetState(PlayerLoginState.Success);
                    LoginSuccessful?.Invoke(currentEmail, currentPlayerName ?? "", currentPlayerData);
                    break;

                default:
                    Debug.Log($"[PlayerLoginPresenter] Error de login: {result}");
                    SetState(PlayerLoginState.Error);
                    view.StatusMessage = message;
                    break;
            }
        }

        private void OnRegisterWithEmailCallback(RegisterResult result, string message, IPlayerCredentials playerData)
        {
            // Solo procesar si este Presenter está esperando un callback de registro
            if (currentState != PlayerLoginState.Validating)
            {
                Debug.Log($"[PlayerLoginPresenter] Ignorando callback de registro - estado actual: {currentState}");
                return;
            }
            
            Debug.Log($"[PlayerLoginPresenter] OnRegisterWithEmailCallback: result={result}, message={message}");
            
            switch (result)
            {
                case RegisterResult.Success:
                    // Cuenta creada - login exitoso directo
                    // CharacterViewController se encargará de crear el personaje
                    Debug.Log($"[PlayerLoginPresenter] Registro exitoso, disparando LoginSuccessful para {currentEmail}");
                    currentPlayerData = playerData;
                    SetState(PlayerLoginState.Success);
                    LoginSuccessful?.Invoke(currentEmail, currentPlayerName ?? "", currentPlayerData);
                    break;

                case RegisterResult.EmailAlreadyExists:
                    // Email ya existe, ir a login
                    SetState(PlayerLoginState.EnterPasswordForLogin);
                    view.StatusMessage = "Este email ya está registrado. Ingresa tu contraseña.";
                    break;

                case RegisterResult.InvalidPassword:
                    // Contraseña inválida, volver a pedir
                    SetState(PlayerLoginState.EnterPasswordForNewUser);
                    view.StatusMessage = message;
                    break;

                case RegisterResult.NameAlreadyExists:
                    SetState(PlayerLoginState.EnterRegistrationData);
                    view.StatusMessage = message;
                    break;

                default:
                    SetState(PlayerLoginState.Error);
                    view.StatusMessage = message;
                    break;
            }
        }

        #endregion

        #region v1 Legacy Handlers

        private void HandleNameSubmitted(string playerName)
        {
            if (validator != null && !validator.IsValidPlayerName(playerName))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPlayerName);
                return;
            }

            currentPlayerName = playerName.Trim();
            SetState(PlayerLoginState.CheckingName);

            if (loginApi != null)
            {
                loginApi.CheckPlayerExists(currentPlayerName);
            }
            else
            {
                view.StatusMessage = "Error: API no disponible";
                SetState(PlayerLoginState.Error);
            }
        }

        private void HandlePasswordSubmitted(string password)
        {
            if (validator != null && !validator.IsValidPasswordFormat(password))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPasswordFormat);
                return;
            }

            SetState(PlayerLoginState.Validating);

            if (loginApi != null)
            {
                loginApi.Login(currentPlayerName, password);
            }
            else
            {
                view.StatusMessage = "Error: API no disponible";
                SetState(PlayerLoginState.Error);
            }
        }

        private void OnCheckPlayerExistsCallback(bool exists, string playerName)
        {
            if (playerName != currentPlayerName) return;

            if (exists)
            {
                if (loginApi.IsPlayerBlocked(playerName))
                {
                    SetState(PlayerLoginState.Blocked);
                }
                else
                {
                    SetState(PlayerLoginState.EnterPassword);
                }
            }
            else
            {
                SetState(PlayerLoginState.NewPlayer);
            }
        }

        private void OnLoginCallback(LoginResult result, int remainingAttempts, string message)
        {
            switch (result)
            {
                case LoginResult.Success:
                    SetState(PlayerLoginState.Success);
                    LoginSuccessful?.Invoke(null, currentPlayerName, null);
                    break;

                case LoginResult.WrongPassword:
                    SetState(PlayerLoginState.EnterPassword);
                    view.StatusMessage = message;
                    view.ClearInput();
                    view.FocusInput();
                    break;

                case LoginResult.Blocked:
                    SetState(PlayerLoginState.Blocked);
                    view.StatusMessage = message;
                    break;

                default:
                    SetState(PlayerLoginState.Error);
                    view.StatusMessage = message;
                    break;
            }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Reinicia el presenter al estado inicial.
        /// </summary>
        public void Reset()
        {
            currentEmail = null;
            currentPlayerName = null;
            selectedCharacterClass = null;
            currentPlayerData = null;
            SetState(PlayerLoginState.EnterEmail);
        }

        /// <summary>
        /// Reinicia al estado inicial v1 (legacy).
        /// </summary>
        public void ResetToLegacyMode()
        {
            currentEmail = null;
            currentPlayerName = null;
            selectedCharacterClass = null;
            currentPlayerData = null;
            SetState(PlayerLoginState.EnterName);
        }

        #endregion
    }
}
