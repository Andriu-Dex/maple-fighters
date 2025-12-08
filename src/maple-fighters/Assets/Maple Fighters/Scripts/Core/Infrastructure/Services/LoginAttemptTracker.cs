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
        public bool RecordFailedAttempt(string playerNameOrEmail)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerNameOrEmail))
            {
                return false;
            }

            // Detectar si es un email (contiene '@') y usar el método apropiado
            var failedAttempts = playerNameOrEmail.Contains("@")
                ? playerRepository.IncrementFailedAttemptsByEmail(playerNameOrEmail)
                : playerRepository.IncrementFailedAttempts(playerNameOrEmail);
            var isNowBlocked = failedAttempts >= MaxAttempts;

            if (isNowBlocked)
            {
                Debug.Log($"[LoginAttemptTracker] Jugador bloqueado: {playerNameOrEmail} (intentos: {failedAttempts})");
            }
            else
            {
                Debug.Log($"[LoginAttemptTracker] Intento fallido: {playerNameOrEmail} ({failedAttempts}/{MaxAttempts})");
            }

            return isNowBlocked;
        }

        /// <inheritdoc/>
        public void RecordSuccessfulLogin(string playerNameOrEmail)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerNameOrEmail))
            {
                return;
            }

            // Detectar si es un email (contiene '@') y usar el método apropiado
            if (playerNameOrEmail.Contains("@"))
            {
                playerRepository.ResetFailedAttemptsByEmail(playerNameOrEmail);
            }
            else
            {
                playerRepository.ResetFailedAttempts(playerNameOrEmail);
            }
            Debug.Log($"[LoginAttemptTracker] Login exitoso, intentos reseteados: {playerNameOrEmail}");
        }

        /// <inheritdoc/>
        public int GetFailedAttempts(string playerNameOrEmail)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerNameOrEmail))
            {
                return 0;
            }

            // Detectar si es un email (contiene '@') y usar el método apropiado
            var player = playerNameOrEmail.Contains("@") 
                ? playerRepository.GetPlayerByEmail(playerNameOrEmail)
                : playerRepository.GetPlayer(playerNameOrEmail);
            return player?.FailedAttempts ?? 0;
        }

        /// <inheritdoc/>
        public int GetRemainingAttempts(string playerName)
        {
            var failedAttempts = GetFailedAttempts(playerName);
            return System.Math.Max(0, MaxAttempts - failedAttempts);
        }

        /// <inheritdoc/>
        public bool IsBlocked(string playerNameOrEmail)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerNameOrEmail))
            {
                return false;
            }

            // Detectar si es un email (contiene '@') y usar el método apropiado
            var player = playerNameOrEmail.Contains("@") 
                ? playerRepository.GetPlayerByEmail(playerNameOrEmail)
                : playerRepository.GetPlayer(playerNameOrEmail);
            return player?.IsBlocked ?? false;
        }

        /// <inheritdoc/>
        public void Unblock(string playerNameOrEmail)
        {
            if (playerRepository == null || string.IsNullOrEmpty(playerNameOrEmail))
            {
                return;
            }

            // Detectar si es un email (contiene '@') y usar el método apropiado
            if (playerNameOrEmail.Contains("@"))
            {
                playerRepository.UnblockPlayerByEmail(playerNameOrEmail);
                Debug.Log($"[LoginAttemptTracker] Jugador desbloqueado por email: {playerNameOrEmail}");
            }
            else
            {
                playerRepository.UnblockPlayer(playerNameOrEmail);
                Debug.Log($"[LoginAttemptTracker] Jugador desbloqueado por nombre: {playerNameOrEmail}");
            }
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
