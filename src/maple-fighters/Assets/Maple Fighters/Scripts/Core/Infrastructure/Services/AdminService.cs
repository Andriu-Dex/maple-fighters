using System.Collections.Generic;
using Scripts.Core.Domain.Interfaces;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Services
{
    /// <summary>
    /// Implementación del servicio de administración.
    /// </summary>
    public class AdminService : IAdminService
    {
        private readonly IPlayerRepository playerRepository;
        private readonly ILoginAttemptTracker attemptTracker;

        /// <inheritdoc/>
        public string AdminPlayerName => PlayerLoginSettings.AdminPlayerName;

        /// <summary>
        /// Constructor que recibe las dependencias.
        /// </summary>
        /// <param name="playerRepository">Repositorio de jugadores.</param>
        /// <param name="attemptTracker">Tracker de intentos de login.</param>
        public AdminService(IPlayerRepository playerRepository, ILoginAttemptTracker attemptTracker)
        {
            this.playerRepository = playerRepository;
            this.attemptTracker = attemptTracker;
        }

        /// <inheritdoc/>
        public bool IsAdmin(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return false;
            }

            return playerName.Equals(AdminPlayerName, System.StringComparison.OrdinalIgnoreCase);
        }

        /// <inheritdoc/>
        public bool UnblockAllPlayers(string adminPlayerName)
        {
            if (!IsAdmin(adminPlayerName))
            {
                Debug.LogWarning($"[AdminService] Intento de desbloqueo no autorizado por: {adminPlayerName}");
                return false;
            }

            attemptTracker?.UnblockAll();
            Debug.Log($"[AdminService] Todos los jugadores desbloqueados por: {adminPlayerName}");
            return true;
        }

        /// <inheritdoc/>
        public bool UnblockPlayer(string adminPlayerName, string playerName)
        {
            if (!IsAdmin(adminPlayerName))
            {
                Debug.LogWarning($"[AdminService] Intento de desbloqueo no autorizado por: {adminPlayerName}");
                return false;
            }

            if (string.IsNullOrEmpty(playerName))
            {
                return false;
            }

            attemptTracker?.Unblock(playerName);
            Debug.Log($"[AdminService] Jugador {playerName} desbloqueado por: {adminPlayerName}");
            return true;
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetBlockedPlayers(string adminPlayerName)
        {
            if (!IsAdmin(adminPlayerName))
            {
                return new List<string>().AsReadOnly();
            }

            return attemptTracker?.GetBlockedPlayers() ?? new List<string>().AsReadOnly();
        }

        /// <inheritdoc/>
        public int GetBlockedPlayersCount()
        {
            return attemptTracker?.GetBlockedPlayers()?.Count ?? 0;
        }
    }
}
