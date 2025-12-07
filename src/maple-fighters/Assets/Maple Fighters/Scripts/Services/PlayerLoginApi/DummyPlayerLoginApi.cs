using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using UnityEngine;

namespace Scripts.Services.PlayerLoginApi
{
    /// <summary>
    /// Implementación dummy/local de IPlayerLoginApi.
    /// Usa IPlayerRepository e ICredentialValidator del ServiceLocator.
    /// Usa lazy initialization para evitar problemas de orden de ejecución.
    /// </summary>
    public class DummyPlayerLoginApi : MonoBehaviour, IPlayerLoginApi
    {
        private static DummyPlayerLoginApi instance;

        private IPlayerRepository cachedPlayerRepository;
        private ICredentialValidator cachedCredentialValidator;
        private bool servicesInitialized;

        /// <summary>
        /// Obtiene el PlayerRepository con lazy initialization.
        /// </summary>
        private IPlayerRepository PlayerRepository
        {
            get
            {
                EnsureServicesInitialized();
                return cachedPlayerRepository;
            }
        }

        /// <summary>
        /// Obtiene el CredentialValidator con lazy initialization.
        /// </summary>
        private ICredentialValidator CredentialValidator
        {
            get
            {
                EnsureServicesInitialized();
                return cachedCredentialValidator;
            }
        }

        /// <inheritdoc/>
        public Action<bool, string> CheckPlayerExistsCallback { get; set; }

        /// <inheritdoc/>
        public Action<LoginResult, int, string> LoginCallback { get; set; }

        /// <inheritdoc/>
        public Action<RegisterResult, string> RegisterCallback { get; set; }

        /// <summary>
        /// Obtiene la instancia singleton de la API.
        /// </summary>
        public static DummyPlayerLoginApi GetInstance()
        {
            if (instance == null)
            {
                var gameObject = new GameObject("Dummy PlayerLogin Api");
                instance = gameObject.AddComponent<DummyPlayerLoginApi>();
            }
            return instance;
        }

        /// <summary>
        /// Inicializa los servicios de forma lazy (cuando se necesitan por primera vez).
        /// Esto evita problemas de orden de ejecución de Awake().
        /// </summary>
        private void EnsureServicesInitialized()
        {
            if (servicesInitialized) return;

            ServiceLocator.TryGet(out cachedPlayerRepository);
            ServiceLocator.TryGet(out cachedCredentialValidator);

            if (cachedPlayerRepository == null)
            {
                Debug.LogWarning("[DummyPlayerLoginApi] IPlayerRepository no encontrado en ServiceLocator. ¿ServiceLocatorInitializer está en la escena?");
            }

            if (cachedCredentialValidator == null)
            {
                Debug.LogWarning("[DummyPlayerLoginApi] ICredentialValidator no encontrado en ServiceLocator. ¿ServiceLocatorInitializer está en la escena?");
            }

            servicesInitialized = true;
        }

        private void OnDestroy()
        {
            // Limpiar referencia cuando se destruye
            if (instance == this)
            {
                instance = null;
            }
        }

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public void Login(string playerName, string password)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                LoginCallback?.Invoke(LoginResult.Error, 0, "Servicios no disponibles");
                return;
            }

            // Validar credenciales
            var validationResult = CredentialValidator.ValidateCredentials(playerName, password);
            var message = CredentialValidator.GetValidationMessage(validationResult);
            var remainingAttempts = GetRemainingAttempts(playerName);

            LoginResult loginResult;

            switch (validationResult)
            {
                case CredentialValidationResult.Valid:
                    // Login exitoso - resetear intentos
                    PlayerRepository.ResetFailedAttempts(playerName);
                    loginResult = LoginResult.Success;
                    Debug.Log($"[DummyPlayerLoginApi] Login exitoso: {playerName}");
                    break;

                case CredentialValidationResult.PlayerNotFound:
                    loginResult = LoginResult.PlayerNotFound;
                    break;

                case CredentialValidationResult.InvalidPassword:
                    // Incrementar intentos fallidos
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

        /// <inheritdoc/>
        public void Register(string playerName, string password)
        {
            if (PlayerRepository == null || CredentialValidator == null)
            {
                RegisterCallback?.Invoke(RegisterResult.Error, "Servicios no disponibles");
                return;
            }

            // Validar formato del nombre
            if (!CredentialValidator.IsValidPlayerName(playerName))
            {
                var message = CredentialValidator.GetValidationMessage(CredentialValidationResult.InvalidPlayerName);
                RegisterCallback?.Invoke(RegisterResult.InvalidName, message);
                return;
            }

            // Validar formato de contraseña
            if (!CredentialValidator.IsValidPasswordFormat(password))
            {
                var message = CredentialValidator.GetValidationMessage(CredentialValidationResult.InvalidPasswordFormat);
                RegisterCallback?.Invoke(RegisterResult.InvalidPassword, message);
                return;
            }

            // Verificar si ya existe
            if (PlayerRepository.PlayerExists(playerName))
            {
                RegisterCallback?.Invoke(RegisterResult.NameAlreadyExists, "Este nombre ya está en uso");
                return;
            }

            // Registrar nuevo jugador
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

        /// <inheritdoc/>
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

        /// <inheritdoc/>
        public bool IsPlayerBlocked(string playerName)
        {
            if (PlayerRepository == null)
            {
                return false;
            }

            var player = PlayerRepository.GetPlayer(playerName);
            return player?.IsBlocked ?? false;
        }
    }
}
