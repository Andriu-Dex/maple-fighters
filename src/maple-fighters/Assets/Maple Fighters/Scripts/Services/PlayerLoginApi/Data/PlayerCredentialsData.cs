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
        private string playerName;

        [SerializeField]
        private string password;

        [SerializeField]
        private bool isBlocked;

        [SerializeField]
        private int failedAttempts;

        /// <summary>
        /// Nombre único del jugador.
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
        /// Constructor por defecto requerido para serialización.
        /// </summary>
        public PlayerCredentialsData()
        {
        }

        /// <summary>
        /// Constructor con parámetros.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <param name="password">Contraseña.</param>
        public PlayerCredentialsData(string playerName, string password)
        {
            this.playerName = playerName;
            this.password = password;
            this.isBlocked = false;
            this.failedAttempts = 0;
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
            return $"Player: {playerName}, Blocked: {isBlocked}, FailedAttempts: {failedAttempts}";
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
