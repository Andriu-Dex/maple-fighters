using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using UnityEngine;

namespace Scripts.Services.PlayerLoginApi
{
    /// <summary>
    /// Implementación dummy/local de IPlayerLoginApi.
    /// Usa IPlayerRepository e ICredentialValidator del ServiceLocator.
    /// Soporta tanto el flujo v2 (email) como v1 (nombre).
    /// </summary>
    public class DummyPlayerLoginApi : MonoBehaviour, IPlayerLoginApi
    {
        private static DummyPlayerLoginApi instance;

        private IPlayerRepository cachedPlayerRepository;
        private ICredentialValidator cachedCredentialValidator;
        private bool servicesInitialized;

        private IPlayerRepository PlayerRepository
        {
            get
            {
                EnsureServicesInitialized();
                return cachedPlayerRepository;
            }
        }

        private ICredentialValidator CredentialValidator
        {
            get
            {
                EnsureServicesInitialized();
                return cachedCredentialValidator;
            }
        }

        #region v2 Callbacks

        public Action<EmailCheckResult, string, IPlayerCredentials> CheckEmailCallback { get; set; }
        public Action<LoginResult, int, string, IPlayerCredentials> LoginByEmailCallback { get; set; }
        public Action<RegisterResult, string, IPlayerCredentials> RegisterWithEmailCallback { get; set; }

        #endregion

        #region v1 Callbacks (Legacy)

        public Action<bool, string> CheckPlayerExistsCallback { get; set; }
        public Action<LoginResult, int, string> LoginCallback { get; set; }
        public Action<RegisterResult, string> RegisterCallback { get; set; }

        #endregion

        public static DummyPlayerLoginApi GetInstance()
        {
            if (instance == null)
            {
                var gameObject = new GameObject("Dummy PlayerLogin Api");
                instance = gameObject.AddComponent<DummyPlayerLoginApi>();
            }
            return instance;
        }

        private void EnsureServicesInitialized()
        {
            if (servicesInitialized) return;

            ServiceLocator.TryGet(out cachedPlayerRepository);
            ServiceLocator.TryGet(out cachedCredentialValidator);

            if (cachedPlayerRepository == null)
            {
                Debug.LogWarning("[DummyPlayerLoginApi] IPlayerRepository no encontrado en ServiceLocator");
            }

            if (cachedCredentialValidator == null)
            {
                Debug.LogWarning("[DummyPlayerLoginApi] ICredentialValidator no encontrado en ServiceLocator");
            }

            servicesInitialized = true;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }

        #region v2 Methods (Email-based)

        public void CheckEmail(string email)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                CheckEmailCallback?.Invoke(EmailCheckResult.Error, email, null);
                return;
            }

            // Validar formato
            if (!CredentialValidator.IsValidEmail(email))
            {
                Debug.Log($"[DummyPlayerLoginApi] CheckEmail: formato inválido - {email}");
                CheckEmailCallback?.Invoke(EmailCheckResult.InvalidFormat, email, null);
                return;
            }

            // Buscar jugador
            var player = PlayerRepository.GetPlayerByEmail(email);

            if (player == null)
            {
                Debug.Log($"[DummyPlayerLoginApi] CheckEmail: no existe - {email}");
                CheckEmailCallback?.Invoke(EmailCheckResult.NotExists, email, null);
                return;
            }

            // Verificar si el registro está completo
            var credentials = player as PlayerCredentialsData;
            if (credentials != null && credentials.IsRegistrationComplete())
            {
                Debug.Log($"[DummyPlayerLoginApi] CheckEmail: existe completo - {email}");
                CheckEmailCallback?.Invoke(EmailCheckResult.ExistsComplete, email, player);
            }
            else
            {
                Debug.Log($"[DummyPlayerLoginApi] CheckEmail: existe incompleto - {email}");
                CheckEmailCallback?.Invoke(EmailCheckResult.ExistsIncomplete, email, player);
            }
        }

        public void LoginByEmail(string email, string password)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                LoginByEmailCallback?.Invoke(LoginResult.Error, 0, "Servicios no disponibles", null);
                return;
            }

            var validationResult = CredentialValidator.ValidateCredentialsByEmail(email, password);
            var message = CredentialValidator.GetValidationMessage(validationResult);
            var remainingAttempts = GetRemainingAttemptsByEmail(email);

            LoginResult loginResult;
            IPlayerCredentials playerData = null;

            switch (validationResult)
            {
                case CredentialValidationResult.Valid:
                    PlayerRepository.ResetFailedAttemptsByEmail(email);
                    PlayerRepository.UpdateLastLogin(email);
                    playerData = PlayerRepository.GetPlayerByEmail(email);
                    loginResult = LoginResult.Success;
                    Debug.Log($"[DummyPlayerLoginApi] Login exitoso: {email}");
                    break;

                case CredentialValidationResult.PlayerNotFound:
                    loginResult = LoginResult.PlayerNotFound;
                    break;

                case CredentialValidationResult.InvalidPassword:
                    PlayerRepository.IncrementFailedAttemptsByEmail(email);
                    remainingAttempts = GetRemainingAttemptsByEmail(email);
                    
                    if (remainingAttempts <= 0)
                    {
                        loginResult = LoginResult.Blocked;
                        message = "Cuenta bloqueada por demasiados intentos fallidos";
                    }
                    else
                    {
                        loginResult = LoginResult.WrongPassword;
                        message = $"Contraseña incorrecta. Intentos restantes: {remainingAttempts}";
                    }
                    Debug.Log($"[DummyPlayerLoginApi] Login fallido: {email}, intentos restantes: {remainingAttempts}");
                    break;

                case CredentialValidationResult.PlayerBlocked:
                    loginResult = LoginResult.Blocked;
                    break;

                case CredentialValidationResult.InvalidEmailFormat:
                    loginResult = LoginResult.InvalidEmail;
                    break;

                case CredentialValidationResult.InvalidPasswordFormat:
                    loginResult = LoginResult.InvalidPassword;
                    break;

                case CredentialValidationResult.RegistrationIncomplete:
                    loginResult = LoginResult.RegistrationIncomplete;
                    break;

                default:
                    loginResult = LoginResult.Error;
                    break;
            }

            LoginByEmailCallback?.Invoke(loginResult, remainingAttempts, message, playerData);
        }

        public void CreateAccountWithEmail(string email)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                RegisterWithEmailCallback?.Invoke(RegisterResult.Error, "Servicios no disponibles", null);
                return;
            }

            // Validar formato
            if (!CredentialValidator.IsValidEmail(email))
            {
                var message = CredentialValidator.GetValidationMessage(CredentialValidationResult.InvalidEmailFormat);
                RegisterWithEmailCallback?.Invoke(RegisterResult.InvalidEmail, message, null);
                return;
            }

            // Verificar si ya existe
            if (PlayerRepository.EmailExists(email))
            {
                RegisterWithEmailCallback?.Invoke(RegisterResult.EmailAlreadyExists, "Este email ya está registrado", null);
                return;
            }

            // Crear cuenta
            var player = PlayerRepository.CreatePlayerWithEmail(email);

            if (player != null)
            {
                Debug.Log($"[DummyPlayerLoginApi] Cuenta creada con email: {email}");
                RegisterWithEmailCallback?.Invoke(RegisterResult.Success, "Cuenta creada. Seleccione su personaje.", player);
            }
            else
            {
                RegisterWithEmailCallback?.Invoke(RegisterResult.Error, "Error al crear cuenta", null);
            }
        }

        public void CompleteRegistration(string email, string playerName, string password, string characterClass)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                RegisterWithEmailCallback?.Invoke(RegisterResult.Error, "Servicios no disponibles", null);
                return;
            }

            // Validar datos
            var validationResult = CredentialValidator.ValidateRegistrationData(email, playerName, password, characterClass);

            if (validationResult != CredentialValidationResult.Valid)
            {
                var message = CredentialValidator.GetValidationMessage(validationResult);
                RegisterResult result;

                switch (validationResult)
                {
                    case CredentialValidationResult.InvalidEmailFormat:
                        result = RegisterResult.InvalidEmail;
                        break;
                    case CredentialValidationResult.InvalidPlayerName:
                        result = RegisterResult.InvalidName;
                        break;
                    case CredentialValidationResult.InvalidPasswordFormat:
                        result = RegisterResult.InvalidPassword;
                        break;
                    case CredentialValidationResult.PlayerNameAlreadyExists:
                        result = RegisterResult.NameAlreadyExists;
                        break;
                    default:
                        result = RegisterResult.Error;
                        break;
                }

                RegisterWithEmailCallback?.Invoke(result, message, null);
                return;
            }

            // Completar registro
            var success = PlayerRepository.CompleteRegistration(email, playerName, password, characterClass);

            if (success)
            {
                var player = PlayerRepository.GetPlayerByEmail(email);
                Debug.Log($"[DummyPlayerLoginApi] Registro completado: {email} -> {playerName} ({characterClass})");
                RegisterWithEmailCallback?.Invoke(RegisterResult.Success, "Registro completado", player);
            }
            else
            {
                RegisterWithEmailCallback?.Invoke(RegisterResult.Error, "Error al completar registro", null);
            }
        }

        public bool IsEmailBlocked(string email)
        {
            if (PlayerRepository == null)
            {
                return false;
            }

            var player = PlayerRepository.GetPlayerByEmail(email);
            return player?.IsBlocked ?? false;
        }

        public int GetRemainingAttemptsByEmail(string email)
        {
            if (PlayerRepository == null)
            {
                return 0;
            }

            var player = PlayerRepository.GetPlayerByEmail(email);
            if (player == null)
            {
                return PlayerLoginSettings.MaxLoginAttempts;
            }

            return Math.Max(0, PlayerLoginSettings.MaxLoginAttempts - player.FailedAttempts);
        }

        #endregion

        #region v1 Methods (Legacy - PlayerName-based)

        public void CheckPlayerExists(string playerName)
        {
            if (PlayerRepository == null)
            {
                CheckPlayerExistsCallback?.Invoke(false, playerName);
                return;
            }

            var exists = PlayerRepository.PlayerExists(playerName);
            Debug.Log($"[DummyPlayerLoginApi] CheckPlayerExists: {playerName} = {exists}");
            
            CheckPlayerExistsCallback?.Invoke(exists, playerName);
        }

        public void Login(string playerName, string password)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                LoginCallback?.Invoke(LoginResult.Error, 0, "Servicios no disponibles");
                return;
            }

            var validationResult = CredentialValidator.ValidateCredentials(playerName, password);
            var message = CredentialValidator.GetValidationMessage(validationResult);
            var remainingAttempts = GetRemainingAttempts(playerName);

            LoginResult loginResult;

            switch (validationResult)
            {
                case CredentialValidationResult.Valid:
                    PlayerRepository.ResetFailedAttempts(playerName);
                    loginResult = LoginResult.Success;
                    Debug.Log($"[DummyPlayerLoginApi] Login exitoso: {playerName}");
                    break;

                case CredentialValidationResult.PlayerNotFound:
                    loginResult = LoginResult.PlayerNotFound;
                    break;

                case CredentialValidationResult.InvalidPassword:
                    PlayerRepository.IncrementFailedAttempts(playerName);
                    remainingAttempts = GetRemainingAttempts(playerName);
                    
                    if (remainingAttempts <= 0)
                    {
                        loginResult = LoginResult.Blocked;
                        message = "Jugador bloqueado por demasiados intentos fallidos";
                    }
                    else
                    {
                        loginResult = LoginResult.WrongPassword;
                        message = $"Contraseña incorrecta. Intentos restantes: {remainingAttempts}";
                    }
                    Debug.Log($"[DummyPlayerLoginApi] Login fallido: {playerName}, intentos restantes: {remainingAttempts}");
                    break;

                case CredentialValidationResult.PlayerBlocked:
                    loginResult = LoginResult.Blocked;
                    break;

                case CredentialValidationResult.InvalidPlayerName:
                    loginResult = LoginResult.InvalidName;
                    break;

                case CredentialValidationResult.InvalidPasswordFormat:
                    loginResult = LoginResult.InvalidPassword;
                    break;

                default:
                    loginResult = LoginResult.Error;
                    break;
            }

            LoginCallback?.Invoke(loginResult, remainingAttempts, message);
        }

        public void Register(string playerName, string password)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                RegisterCallback?.Invoke(RegisterResult.Error, "Servicios no disponibles");
                return;
            }

            if (!CredentialValidator.IsValidPlayerName(playerName))
            {
                var message = CredentialValidator.GetValidationMessage(CredentialValidationResult.InvalidPlayerName);
                RegisterCallback?.Invoke(RegisterResult.InvalidName, message);
                return;
            }

            if (!CredentialValidator.IsValidPasswordFormat(password))
            {
                var message = CredentialValidator.GetValidationMessage(CredentialValidationResult.InvalidPasswordFormat);
                RegisterCallback?.Invoke(RegisterResult.InvalidPassword, message);
                return;
            }

            if (PlayerRepository.PlayerExists(playerName))
            {
                RegisterCallback?.Invoke(RegisterResult.NameAlreadyExists, "Este nombre ya está en uso");
                return;
            }

            var success = PlayerRepository.RegisterPlayer(playerName, password);

            if (success)
            {
                Debug.Log($"[DummyPlayerLoginApi] Jugador registrado: {playerName}");
                RegisterCallback?.Invoke(RegisterResult.Success, "Registro exitoso");
            }
            else
            {
                RegisterCallback?.Invoke(RegisterResult.Error, "Error al registrar jugador");
            }
        }

        public int GetRemainingAttempts(string playerName)
        {
            if (PlayerRepository == null)
            {
                return 0;
            }

            var player = PlayerRepository.GetPlayer(playerName);
            if (player == null)
            {
                return PlayerLoginSettings.MaxLoginAttempts;
            }

            return Math.Max(0, PlayerLoginSettings.MaxLoginAttempts - player.FailedAttempts);
        }

        public bool IsPlayerBlocked(string playerName)
        {
            if (PlayerRepository == null)
            {
                return false;
            }

            var player = PlayerRepository.GetPlayer(playerName);
            return player?.IsBlocked ?? false;
        }

        #endregion
    }
}
