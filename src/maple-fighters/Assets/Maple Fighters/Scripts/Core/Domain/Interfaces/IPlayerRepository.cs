using System.Collections.Generic;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Repository para gestionar el almacenamiento y recuperación de jugadores.
    /// Sigue el patrón Repository para abstraer la persistencia de datos.
    /// </summary>
    public interface IPlayerRepository
    {
        /// <summary>
        /// Verifica si existe un jugador con el nombre especificado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador a buscar.</param>
        /// <returns>True si el jugador existe, false en caso contrario.</returns>
        bool PlayerExists(string playerName);

        /// <summary>
        /// Obtiene las credenciales de un jugador por su nombre.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>Las credenciales del jugador o null si no existe.</returns>
        IPlayerCredentials GetPlayer(string playerName);

        /// <summary>
        /// Registra un nuevo jugador con las credenciales especificadas.
        /// </summary>
        /// <param name="playerName">Nombre único del jugador.</param>
        /// <param name="password">Contraseña del jugador.</param>
        /// <returns>True si el registro fue exitoso, false si el nombre ya existe.</returns>
        bool RegisterPlayer(string playerName, string password);

        /// <summary>
        /// Actualiza la contraseña de un jugador existente.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <param name="newPassword">Nueva contraseña.</param>
        /// <returns>True si la actualización fue exitosa.</returns>
        bool UpdatePassword(string playerName, string newPassword);

        /// <summary>
        /// Incrementa el contador de intentos fallidos de un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>El nuevo número de intentos fallidos.</returns>
        int IncrementFailedAttempts(string playerName);

        /// <summary>
        /// Reinicia el contador de intentos fallidos de un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        void ResetFailedAttempts(string playerName);

        /// <summary>
        /// Bloquea a un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador a bloquear.</param>
        void BlockPlayer(string playerName);

        /// <summary>
        /// Desbloquea a un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador a desbloquear.</param>
        void UnblockPlayer(string playerName);

        /// <summary>
        /// Desbloquea a todos los jugadores bloqueados.
        /// </summary>
        void UnblockAllPlayers();

        /// <summary>
        /// Obtiene la lista de todos los jugadores bloqueados.
        /// </summary>
        /// <returns>Lista de nombres de jugadores bloqueados.</returns>
        IReadOnlyList<string> GetBlockedPlayers();
    }
}
