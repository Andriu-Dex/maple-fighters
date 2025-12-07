using Scripts.Core.Domain.Interfaces;
using Scripts.Services.PlayerLoginApi;

namespace Scripts.Core.Infrastructure.Services
{
    /// <summary>
    /// Implementación del validador de credenciales.
    /// Usa IPlayerRepository para verificar datos de jugadores.
    /// </summary>
    public class CredentialValidator : ICredentialValidator
    {
        private readonly IPlayerRepository playerRepository;

        /// <summary>
        /// Constructor que recibe el repositorio de jugadores.
        /// </summary>
        /// <param name="playerRepository">Repositorio de jugadores.</param>
        public CredentialValidator(IPlayerRepository playerRepository)
        {
            this.playerRepository = playerRepository;
        }

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

        /// <inheritdoc/>
        public string GetValidationMessage(CredentialValidationResult result)
        {
            switch (result)
            {
                case CredentialValidationResult.Valid:
                    return "Login exitoso";
                
                case CredentialValidationResult.PlayerNotFound:
                    return "Jugador no encontrado";
                
                case CredentialValidationResult.InvalidPassword:
                    return "Contraseña incorrecta";
                
                case CredentialValidationResult.PlayerBlocked:
                    return "Jugador bloqueado por demasiados intentos fallidos";
                
                case CredentialValidationResult.InvalidPlayerName:
                    return $"Nombre inválido. Debe tener entre {PlayerLoginSettings.MinPlayerNameLength} y {PlayerLoginSettings.MaxPlayerNameLength} caracteres (letras, números, _ o -)";
                
                case CredentialValidationResult.InvalidPasswordFormat:
                    return $"Contraseña inválida. Debe tener entre {PlayerLoginSettings.MinPasswordLength} y {PlayerLoginSettings.MaxPasswordLength} caracteres";
                
                default:
                    return "Error desconocido";
            }
        }
    }
}
