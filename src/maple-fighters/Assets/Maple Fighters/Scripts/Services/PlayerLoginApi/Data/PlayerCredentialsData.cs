using System;
using Scripts.Core.Domain.Interfaces;
using UnityEngine;

namespace Scripts.Services.PlayerLoginApi
{
    /// <summary>
    /// Estructura de datos para almacenar credenciales de jugador.
    /// Serializable para persistencia con JsonUtility.
    /// </summary>
    [Serializable]
    public class PlayerCredentialsData : IPlayerCredentials
    {
        [SerializeField]
        private string id;

        [SerializeField]
        private string email;

        [SerializeField]
        private string playerName;

        [SerializeField]
        private string password;

        [SerializeField]
        private string characterClass;

        [SerializeField]
        private string characterId;

        [SerializeField]
        private bool isBlocked;

        [SerializeField]
        private int failedAttempts;

        [SerializeField]
        private string createdAt;

        [SerializeField]
        private string lastLoginAt;

        /// <summary>
        /// Identificador único del jugador (GUID).
        /// </summary>
        public string Id
        {
            get => id;
            set => id = value;
        }

        /// <summary>
        /// Email único del jugador (usado para login).
        /// </summary>
        public string Email
        {
            get => email;
            set => email = value;
        }

        /// <summary>
        /// Nombre del personaje del jugador.
        /// </summary>
        public string PlayerName
        {
            get => playerName;
            set => playerName = value;
        }

        /// <summary>
        /// Contraseña del jugador.
        /// </summary>
        public string Password
        {
            get => password;
            set => password = value;
        }

        /// <summary>
        /// Clase del personaje seleccionado.
        /// </summary>
        public string CharacterClass
        {
            get => characterClass;
            set => characterClass = value;
        }

        /// <summary>
        /// ID único del personaje para el servidor.
        /// </summary>
        public string CharacterId
        {
            get => characterId;
            set => characterId = value;
        }

        /// <summary>
        /// Indica si el jugador está bloqueado.
        /// </summary>
        public bool IsBlocked
        {
            get => isBlocked;
            set => isBlocked = value;
        }

        /// <summary>
        /// Número de intentos fallidos consecutivos.
        /// </summary>
        public int FailedAttempts
        {
            get => failedAttempts;
            set => failedAttempts = value;
        }

        /// <summary>
        /// Fecha y hora de creación de la cuenta.
        /// </summary>
        public DateTime CreatedAt
        {
            get => string.IsNullOrEmpty(createdAt) ? DateTime.MinValue : DateTime.Parse(createdAt);
            set => createdAt = value.ToString("O");
        }

        /// <summary>
        /// Fecha y hora del último login exitoso.
        /// </summary>
        public DateTime LastLoginAt
        {
            get => string.IsNullOrEmpty(lastLoginAt) ? DateTime.MinValue : DateTime.Parse(lastLoginAt);
            set => lastLoginAt = value.ToString("O");
        }

        /// <summary>
        /// Constructor por defecto requerido para serialización.
        /// </summary>
        public PlayerCredentialsData()
        {
            id = Guid.NewGuid().ToString();
            createdAt = DateTime.UtcNow.ToString("O");
            lastLoginAt = DateTime.UtcNow.ToString("O");
        }

        /// <summary>
        /// Constructor con email para registro inicial (antes de selección de personaje).
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        public PlayerCredentialsData(string email)
        {
            this.id = Guid.NewGuid().ToString();
            this.email = email;
            this.playerName = string.Empty;
            this.password = string.Empty;
            this.characterClass = string.Empty;
            this.characterId = string.Empty;
            this.isBlocked = false;
            this.failedAttempts = 0;
            this.createdAt = DateTime.UtcNow.ToString("O");
            this.lastLoginAt = DateTime.UtcNow.ToString("O");
        }

        /// <summary>
        /// Constructor completo con todos los datos del personaje.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <param name="playerName">Nombre del personaje.</param>
        /// <param name="password">Contraseña.</param>
        /// <param name="characterClass">Clase del personaje.</param>
        public PlayerCredentialsData(string email, string playerName, string password, string characterClass)
        {
            this.id = Guid.NewGuid().ToString();
            this.email = email;
            this.playerName = playerName;
            this.password = password;
            this.characterClass = characterClass;
            this.characterId = Guid.NewGuid().ToString();
            this.isBlocked = false;
            this.failedAttempts = 0;
            this.createdAt = DateTime.UtcNow.ToString("O");
            this.lastLoginAt = DateTime.UtcNow.ToString("O");
        }

        /// <summary>
        /// Constructor legacy para compatibilidad (sin email).
        /// Genera un email placeholder basado en el nombre del jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <param name="password">Contraseña.</param>
        [Obsolete("Use constructor with email parameter for new registrations")]
        public PlayerCredentialsData(string playerName, string password)
        {
            this.id = Guid.NewGuid().ToString();
            this.email = $"{playerName.ToLowerInvariant().Replace(" ", "_")}@legacy.local";
            this.playerName = playerName;
            this.password = password;
            this.characterClass = string.Empty;
            this.characterId = Guid.NewGuid().ToString();
            this.isBlocked = false;
            this.failedAttempts = 0;
            this.createdAt = DateTime.UtcNow.ToString("O");
            this.lastLoginAt = DateTime.UtcNow.ToString("O");
        }

        /// <summary>
        /// Actualiza la fecha de último login a ahora.
        /// </summary>
        public void UpdateLastLogin()
        {
            lastLoginAt = DateTime.UtcNow.ToString("O");
        }

        /// <summary>
        /// Verifica si el jugador ha completado el registro (tiene nombre y contraseña).
        /// </summary>
        /// <returns>True si el registro está completo.</returns>
        public bool IsRegistrationComplete()
        {
            return !string.IsNullOrEmpty(playerName) && !string.IsNullOrEmpty(password);
        }

        /// <summary>
        /// Verifica si el jugador ha seleccionado un personaje.
        /// </summary>
        /// <returns>True si tiene clase de personaje asignada.</returns>
        public bool HasCharacter()
        {
            return !string.IsNullOrEmpty(characterClass);
        }

        /// <summary>
        /// Crea una instancia desde JSON.
        /// </summary>
        /// <param name="json">String JSON.</param>
        /// <returns>Instancia de PlayerCredentialsData.</returns>
        public static PlayerCredentialsData FromJson(string json)
        {
            return JsonUtility.FromJson<PlayerCredentialsData>(json);
        }

        /// <summary>
        /// Convierte la instancia a JSON.
        /// </summary>
        /// <returns>String JSON.</returns>
        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }

        public override string ToString()
        {
            return $"Player: {playerName} (Email: {email}), Class: {characterClass}, Blocked: {isBlocked}, FailedAttempts: {failedAttempts}";
        }
    }

    /// <summary>
    /// Colección de credenciales de jugadores para serialización.
    /// </summary>
    [Serializable]
    public class PlayerCredentialsCollection
    {
        [SerializeField]
        public PlayerCredentialsData[] players;

        public PlayerCredentialsCollection()
        {
            players = Array.Empty<PlayerCredentialsData>();
        }

        public PlayerCredentialsCollection(PlayerCredentialsData[] players)
        {
            this.players = players ?? Array.Empty<PlayerCredentialsData>();
        }

        public static PlayerCredentialsCollection FromJson(string json)
        {
            if (string.IsNullOrEmpty(json))
            {
                return new PlayerCredentialsCollection();
            }
            return JsonUtility.FromJson<PlayerCredentialsCollection>(json);
        }

        public string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
    }
}
