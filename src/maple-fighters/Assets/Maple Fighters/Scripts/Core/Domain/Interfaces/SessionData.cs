using System;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Datos de sesión del jugador para auto-login.
    /// </summary>
    [Serializable]
    public class SessionData
    {
        /// <summary>
        /// Email del jugador.
        /// </summary>
        public string Email;

        /// <summary>
        /// Nombre del jugador.
        /// </summary>
        public string PlayerName;

        /// <summary>
        /// Token de sesión (timestamp de creación).
        /// </summary>
        public long SessionToken;

        /// <summary>
        /// Clase del personaje.
        /// </summary>
        public string CharacterClass;

        /// <summary>
        /// Timestamp de cuando se creó la sesión.
        /// </summary>
        public long CreatedAt;

        /// <summary>
        /// Duración de la sesión en segundos (por defecto 7 días).
        /// </summary>
        public static readonly long DefaultSessionDuration = 7 * 24 * 60 * 60; // 7 días

        /// <summary>
        /// Constructor vacío para serialización.
        /// </summary>
        public SessionData() { }

        /// <summary>
        /// Crea una nueva sesión con los datos del jugador.
        /// </summary>
        public SessionData(string email, string playerName, string characterClass = null)
        {
            Email = email;
            PlayerName = playerName;
            CharacterClass = characterClass;
            CreatedAt = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            SessionToken = CreatedAt;
        }

        /// <summary>
        /// Verifica si la sesión es válida (no ha expirado).
        /// </summary>
        public bool IsValid()
        {
            return IsValid(DefaultSessionDuration);
        }

        /// <summary>
        /// Verifica si la sesión es válida con duración personalizada.
        /// </summary>
        public bool IsValid(long sessionDurationSeconds)
        {
            if (string.IsNullOrEmpty(Email) || SessionToken == 0)
            {
                return false;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            return (now - SessionToken) < sessionDurationSeconds;
        }

        /// <summary>
        /// Renueva el token de sesión.
        /// </summary>
        public void Refresh()
        {
            SessionToken = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        }

        /// <summary>
        /// Tiempo restante de la sesión en segundos.
        /// </summary>
        public long RemainingTime()
        {
            var now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            var elapsed = now - SessionToken;
            return Math.Max(0, DefaultSessionDuration - elapsed);
        }

        /// <summary>
        /// Limpia los datos de la sesión.
        /// </summary>
        public void Clear()
        {
            Email = null;
            PlayerName = null;
            CharacterClass = null;
            SessionToken = 0;
            CreatedAt = 0;
        }

        public override string ToString()
        {
            return $"Session[{Email}:{PlayerName}, valid={IsValid()}, remaining={RemainingTime()}s]";
        }
    }
}
