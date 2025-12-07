using System.Collections.Generic;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para el tracking de intentos de login.
    /// Proporciona una capa de abstracción sobre la lógica de bloqueo.
    /// </summary>
    public interface ILoginAttemptTracker
    {
        /// <summary>
        /// Número máximo de intentos permitidos antes de bloquear.
        /// </summary>
        int MaxAttempts { get; }

        /// <summary>
        /// Registra un intento fallido de login.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>True si el jugador ha sido bloqueado como resultado.</returns>
        bool RecordFailedAttempt(string playerName);

        /// <summary>
        /// Registra un login exitoso (resetea los intentos fallidos).
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        void RecordSuccessfulLogin(string playerName);

        /// <summary>
        /// Obtiene el número de intentos fallidos de un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>Número de intentos fallidos.</returns>
        int GetFailedAttempts(string playerName);

        /// <summary>
        /// Obtiene el número de intentos restantes antes de ser bloqueado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>Intentos restantes.</returns>
        int GetRemainingAttempts(string playerName);

        /// <summary>
        /// Verifica si un jugador está bloqueado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>True si está bloqueado.</returns>
        bool IsBlocked(string playerName);

        /// <summary>
        /// Desbloquea a un jugador específico.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        void Unblock(string playerName);

        /// <summary>
        /// Desbloquea a todos los jugadores bloqueados.
        /// </summary>
        void UnblockAll();

        /// <summary>
        /// Obtiene la lista de todos los jugadores bloqueados.
        /// </summary>
        /// <returns>Lista de nombres de jugadores bloqueados.</returns>
        IReadOnlyList<string> GetBlockedPlayers();
    }
}
