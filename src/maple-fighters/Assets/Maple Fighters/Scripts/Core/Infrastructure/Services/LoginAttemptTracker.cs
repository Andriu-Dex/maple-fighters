using System.Collections.Generic;
using Scripts.Core.Domain.Interfaces;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Services
{
    /// <summary>
    /// Implementación del tracker de intentos de login.
    /// Delega la persistencia al IPlayerRepository.
    /// </summary>
    public class LoginAttemptTracker : ILoginAttemptTracker
    {
        private readonly IPlayerRepository playerRepository;

        /// <inheritdoc/>
        public int MaxAttempts => PlayerLoginSettings.MaxLoginAttempts;

        /// <summary>
        /// Constructor que recibe el repositorio de jugadores.
        /// </summary>
        /// <param name="playerRepository">Repositorio de jugadores.</param>
        public LoginAttemptTracker(IPlayerRepository playerRepository)
        {
            this.playerRepository = playerRepository;
        }

        /// <inheritdoc/>
        public bool RecordFailedAttempt(string playerName)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerName))
            {
                return false;
            }

            var failedAttempts = playerRepository.IncrementFailedAttempts(playerName);
            var isNowBlocked = failedAttempts >= MaxAttempts;

            if (isNowBlocked)
            {
                Debug.Log($"[LoginAttemptTracker] Jugador bloqueado: {playerName} (intentos: {failedAttempts})");
            }
            else
            {
                Debug.Log($"[LoginAttemptTracker] Intento fallido: {playerName} ({failedAttempts}/{MaxAttempts})");
            }

            return isNowBlocked;
        }

        /// <inheritdoc/>
        public void RecordSuccessfulLogin(string playerName)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerName))
            {
                return;
            }

            playerRepository.ResetFailedAttempts(playerName);
            Debug.Log($"[LoginAttemptTracker] Login exitoso, intentos reseteados: {playerName}");
        }

        /// <inheritdoc/>
        public int GetFailedAttempts(string playerName)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerName))
            {
                return 0;
            }

            var player = playerRepository.GetPlayer(playerName);
            return player?.FailedAttempts ?? 0;
        }

        /// <inheritdoc/>
        public int GetRemainingAttempts(string playerName)
        {
            var failedAttempts = GetFailedAttempts(playerName);
            return System.Math.Max(0, MaxAttempts - failedAttempts);
        }

        /// <inheritdoc/>
        public bool IsBlocked(string playerName)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerName))
            {
                return false;
            }

            var player = playerRepository.GetPlayer(playerName);
            return player?.IsBlocked ?? false;
        }

        /// <inheritdoc/>
        public void Unblock(string playerName)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerName))
            {
                return;
            }

            playerRepository.UnblockPlayer(playerName);
            Debug.Log($"[LoginAttemptTracker] Jugador desbloqueado: {playerName}");
        }

        /// <inheritdoc/>
        public void UnblockAll()
        {
            if (playerRepository == null)
            {
                return;
            }

            playerRepository.UnblockAllPlayers();
            Debug.Log("[LoginAttemptTracker] Todos los jugadores desbloqueados");
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetBlockedPlayers()
        {
            if (playerRepository == null)
            {
                return new List<string>().AsReadOnly();
            }

            return playerRepository.GetBlockedPlayers();
        }
    }
}
