using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.Services;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Presenter para la vista de login de jugadores.
    /// Coordina la lógica entre la vista y la API de login.
    /// Sigue el patrón MVP (Model-View-Presenter).
    /// </summary>
    public class PlayerLoginPresenter : IDisposable
    {
        private readonly IPlayerLoginView view;
        private readonly IPlayerLoginApi loginApi;
        private readonly ICredentialValidator validator;

        private PlayerLoginState currentState;
        private string currentPlayerName;

        /// <summary>
        /// Se dispara cuando el login es exitoso.
        /// Parámetro: nombre del jugador.
        /// </summary>
        public event Action<string> LoginSuccessful;

        /// <summary>
        /// Se dispara cuando se necesita crear un nuevo jugador.
        /// Parámetro: nombre del jugador.
        /// </summary>
        public event Action<string> NewPlayerRequested;

        /// <summary>
        /// Se dispara cuando el usuario quiere volver atrás.
        /// </summary>
        public event Action BackRequested;

        /// <summary>
        /// Estado actual del flujo de login.
        /// </summary>
        public PlayerLoginState CurrentState => currentState;

        /// <summary>
        /// Nombre del jugador actual.
        /// </summary>
        public string CurrentPlayerName => currentPlayerName;

        /// <summary>
        /// Constructor del presenter.
        /// </summary>
        /// <param name="view">Vista de login.</param>
        /// <param name="loginApi">API de login.</param>
        /// <param name="validator">Validador de credenciales.</param>
        public PlayerLoginPresenter(
            IPlayerLoginView view, 
            IPlayerLoginApi loginApi, 
            ICredentialValidator validator)
        {
            this.view = view;
            this.loginApi = loginApi;
            this.validator = validator;

            SubscribeToEvents();
            SetState(PlayerLoginState.EnterName);
        }

        /// <summary>
        /// Libera los recursos y desuscribe eventos.
        /// </summary>
        public void Dispose()
        {
            UnsubscribeFromEvents();
        }

        private void SubscribeToEvents()
        {
            if (view != null)
            {
                view.ConfirmButtonClicked += OnConfirmButtonClicked;
                view.BackButtonClicked += OnBackButtonClicked;
                view.InputFieldChanged += OnInputFieldChanged;
            }

            if (loginApi != null)
            {
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
            }

            if (loginApi != null)
            {
                loginApi.CheckPlayerExistsCallback -= OnCheckPlayerExistsCallback;
                loginApi.LoginCallback -= OnLoginCallback;
            }
        }

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

                case PlayerLoginState.Validating:
                    view.StatusMessage = "Validando...";
                    view.DisableInteraction();
                    break;

                case PlayerLoginState.Success:
                    view.StatusMessage = "¡Login exitoso!";
                    view.DisableInteraction();
                    break;

                case PlayerLoginState.NewPlayer:
                    view.StatusMessage = "Creando nuevo personaje...";
                    view.DisableInteraction();
                    break;

                case PlayerLoginState.Blocked:
                    view.StatusMessage = "Jugador bloqueado. Contacta al administrador.";
                    view.DisableConfirmButton();
                    break;

                case PlayerLoginState.Error:
                    view.EnableInteraction();
                    break;
            }
        }

        private void OnConfirmButtonClicked(string input)
        {
            switch (currentState)
            {
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
                case PlayerLoginState.EnterPassword:
                    // Volver a ingresar nombre
                    currentPlayerName = null;
                    SetState(PlayerLoginState.EnterName);
                    break;

                default:
                    // Notificar que quiere volver
                    BackRequested?.Invoke();
                    break;
            }
        }

        private void OnInputFieldChanged(string text)
        {
            if (view == null) return;

            // Habilitar/deshabilitar botón según validación
            bool isValid = false;

            switch (currentState)
            {
                case PlayerLoginState.EnterName:
                    isValid = validator?.IsValidPlayerName(text) ?? !string.IsNullOrEmpty(text);
                    break;

                case PlayerLoginState.EnterPassword:
                    isValid = validator?.IsValidPasswordFormat(text) ?? !string.IsNullOrEmpty(text);
                    break;
            }

            if (isValid)
            {
                view.EnableConfirmButton();
            }
            else
            {
                view.DisableConfirmButton();
            }
        }

        private void HandleNameSubmitted(string playerName)
        {
            if (!validator.IsValidPlayerName(playerName))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPlayerName);
                return;
            }

            currentPlayerName = playerName.Trim();
            SetState(PlayerLoginState.CheckingName);

            // Verificar si el jugador existe
            loginApi?.CheckPlayerExists(currentPlayerName);
        }

        private void HandlePasswordSubmitted(string password)
        {
            if (!validator.IsValidPasswordFormat(password))
            {
                view.StatusMessage = validator.GetValidationMessage(CredentialValidationResult.InvalidPasswordFormat);
                return;
            }

            SetState(PlayerLoginState.Validating);

            // Intentar login
            loginApi?.Login(currentPlayerName, password);
        }

        private void OnCheckPlayerExistsCallback(bool exists, string playerName)
        {
            if (playerName != currentPlayerName) return;

            if (exists)
            {
                // El jugador existe, verificar si está bloqueado
                if (loginApi.IsPlayerBlocked(playerName))
                {
                    SetState(PlayerLoginState.Blocked);
                }
                else
                {
                    // Pedir contraseña
                    SetState(PlayerLoginState.EnterPassword);
                }
            }
            else
            {
                // Jugador nuevo - ir a creación de personaje
                SetState(PlayerLoginState.NewPlayer);
                NewPlayerRequested?.Invoke(currentPlayerName);
            }
        }

        private void OnLoginCallback(LoginResult result, int remainingAttempts, string message)
        {
            switch (result)
            {
                case LoginResult.Success:
                    SetState(PlayerLoginState.Success);
                    LoginSuccessful?.Invoke(currentPlayerName);
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

        /// <summary>
        /// Reinicia el presenter al estado inicial.
        /// </summary>
        public void Reset()
        {
            currentPlayerName = null;
            SetState(PlayerLoginState.EnterName);
        }
    }
}
