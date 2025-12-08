using System.Text.RegularExpressions;
using Scripts.Core.Domain.Interfaces;
using Scripts.Services.PlayerLoginApi;

namespace Scripts.Core.Infrastructure.Services
{
    /// <summary>
    /// Implementación del validador de credenciales.
    /// Usa IPlayerRepository para verificar datos de jugadores.
    /// Soporta validación por email (v2) y por nombre (v1 legacy).
    /// </summary>
    public class CredentialValidator : ICredentialValidator
    {
        private readonly IPlayerRepository playerRepository;

        // Regex para validar formato de email (simplificado pero efectivo)
        private static readonly Regex EmailRegex = new Regex(
            @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$",
            RegexOptions.Compiled | RegexOptions.IgnoreCase
        );

        /// <summary>
        /// Constructor que recibe el repositorio de jugadores.
        /// </summary>
        /// <param name="playerRepository">Repositorio de jugadores.</param>
        public CredentialValidator(IPlayerRepository playerRepository)
        {
            this.playerRepository = playerRepository;
        }

        #region Email Validation (v2)

        /// <inheritdoc/>
        public bool IsValidEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }

            var trimmed = email.Trim();

            // Longitud mínima y máxima
            if (trimmed.Length < 5 || trimmed.Length > 254)
            {
                return false;
            }

            return EmailRegex.IsMatch(trimmed);
        }

        /// <inheritdoc/>
        public CredentialValidationResult ValidateCredentialsByEmail(string email, string password)
        {
            // Validar formato del email
            if (!IsValidEmail(email))
            {
                return CredentialValidationResult.InvalidEmailFormat;
            }

            // Validar formato de contraseña
            if (!IsValidPasswordFormat(password))
            {
                return CredentialValidationResult.InvalidPasswordFormat;
            }

            // Verificar si el jugador existe
            var player = playerRepository.GetPlayerByEmail(email);
            if (player == null)
            {
                return CredentialValidationResult.PlayerNotFound;
            }

            // Verificar si está bloqueado
            if (player.IsBlocked)
            {
                return CredentialValidationResult.PlayerBlocked;
            }

            // Verificar si el registro está completo
            if (string.IsNullOrEmpty(player.Password))
            {
                return CredentialValidationResult.RegistrationIncomplete;
            }

            // Verificar contraseña
            if (player.Password != password)
            {
                return CredentialValidationResult.InvalidPassword;
            }

            return CredentialValidationResult.Valid;
        }

        /// <inheritdoc/>
        public CredentialValidationResult ValidateEmailForRegistration(string email)
        {
            // Validar formato del email
            if (!IsValidEmail(email))
            {
                return CredentialValidationResult.InvalidEmailFormat;
            }

            // Verificar si el email ya existe
            if (playerRepository.EmailExists(email))
            {
                return CredentialValidationResult.EmailAlreadyExists;
            }

            return CredentialValidationResult.Valid;
        }

        /// <inheritdoc/>
        public CredentialValidationResult ValidateRegistrationData(string email, string playerName, string password, string characterClass)
        {
            // Validar email
            if (!IsValidEmail(email))
            {
                return CredentialValidationResult.InvalidEmailFormat;
            }

            // Validar nombre
            if (!IsValidPlayerName(playerName))
            {
                return CredentialValidationResult.InvalidPlayerName;
            }

            // Validar contraseña
            if (!IsValidPasswordFormat(password))
            {
                return CredentialValidationResult.InvalidPasswordFormat;
            }

            // Validar clase de personaje
            if (string.IsNullOrEmpty(characterClass))
            {
                return CredentialValidationResult.RegistrationIncomplete;
            }

            // Verificar si el nombre ya está en uso
            if (playerRepository.PlayerExists(playerName))
            {
                return CredentialValidationResult.PlayerNameAlreadyExists;
            }

            return CredentialValidationResult.Valid;
        }

        #endregion

        #region PlayerName Validation (legacy v1)

        /// <inheritdoc/>
        public bool IsValidPlayerName(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return false;
            }

            var trimmed = playerName.Trim();
            
            if (trimmed.Length < PlayerLoginSettings.MinPlayerNameLength)
            {
                return false;
            }

            if (trimmed.Length > PlayerLoginSettings.MaxPlayerNameLength)
            {
                return false;
            }

            // Solo permitir letras, números y algunos caracteres especiales
            foreach (char c in trimmed)
            {
                if (!char.IsLetterOrDigit(c) && c != '_' && c != '-')
                {
                    return false;
                }
            }

            return true;
        }

        /// <inheritdoc/>
        public bool IsValidPasswordFormat(string password)
        {
            if (string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (password.Length < PlayerLoginSettings.MinPasswordLength)
            {
                return false;
            }

            if (password.Length > PlayerLoginSettings.MaxPasswordLength)
            {
                return false;
            }

            return true;
        }

        /// <inheritdoc/>
        public CredentialValidationResult ValidateCredentials(string playerName, string password)
        {
            // Validar formato del nombre
            if (!IsValidPlayerName(playerName))
            {
                return CredentialValidationResult.InvalidPlayerName;
            }

            // Validar formato de contraseña
            if (!IsValidPasswordFormat(password))
            {
                return CredentialValidationResult.InvalidPasswordFormat;
            }

            // Verificar si el jugador existe
            var player = playerRepository.GetPlayer(playerName);
            if (player == null)
            {
                return CredentialValidationResult.PlayerNotFound;
            }

            // Verificar si está bloqueado
            if (player.IsBlocked)
            {
                return CredentialValidationResult.PlayerBlocked;
            }

            // Verificar contraseña
            if (player.Password != password)
            {
                return CredentialValidationResult.InvalidPassword;
            }

            return CredentialValidationResult.Valid;
        }

        #endregion

        /// <inheritdoc/>
        public string GetValidationMessage(CredentialValidationResult result)
        {
            switch (result)
            {
                case CredentialValidationResult.Valid:
                    return "Validación exitosa";
                
                case CredentialValidationResult.PlayerNotFound:
                    return "Email no registrado";
                
                case CredentialValidationResult.InvalidPassword:
                    return "Contraseña incorrecta";
                
                case CredentialValidationResult.PlayerBlocked:
                    return "Cuenta bloqueada por demasiados intentos fallidos";
                
                case CredentialValidationResult.InvalidPlayerName:
                    return $"Nombre inválido. Debe tener entre {PlayerLoginSettings.MinPlayerNameLength} y {PlayerLoginSettings.MaxPlayerNameLength} caracteres (letras, números, _ o -)";
                
                case CredentialValidationResult.InvalidPasswordFormat:
                    return $"Contraseña inválida. Debe tener entre {PlayerLoginSettings.MinPasswordLength} y {PlayerLoginSettings.MaxPasswordLength} caracteres";
                
                case CredentialValidationResult.InvalidEmailFormat:
                    return "Formato de email inválido";
                
                case CredentialValidationResult.EmailAlreadyExists:
                    return "Este email ya está registrado";
                
                case CredentialValidationResult.PlayerNameAlreadyExists:
                    return "Este nombre de jugador ya está en uso";
                
                case CredentialValidationResult.RegistrationIncomplete:
                    return "Debe completar el registro con nombre y contraseña";
                
                default:
                    return "Error desconocido";
            }
        }
    }
}
