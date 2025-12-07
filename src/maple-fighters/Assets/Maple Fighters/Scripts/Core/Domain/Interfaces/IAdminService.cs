using System.Collections.Generic;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para el servicio de administración.
    /// Proporciona funcionalidades de administrador como desbloquear usuarios.
    /// </summary>
    public interface IAdminService
    {
        /// <summary>
        /// Nombre del administrador.
        /// </summary>
        string AdminPlayerName { get; }

        /// <summary>
        /// Verifica si un jugador es administrador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>True si es administrador.</returns>
        bool IsAdmin(string playerName);

        /// <summary>
        /// Desbloquea a todos los jugadores bloqueados.
        /// Solo puede ser ejecutado por un administrador.
        /// </summary>
        /// <param name="adminPlayerName">Nombre del administrador que ejecuta la acción.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        bool UnblockAllPlayers(string adminPlayerName);

        /// <summary>
        /// Desbloquea a un jugador específico.
        /// Solo puede ser ejecutado por un administrador.
        /// </summary>
        /// <param name="adminPlayerName">Nombre del administrador.</param>
        /// <param name="playerName">Nombre del jugador a desbloquear.</param>
        /// <returns>True si la operación fue exitosa.</returns>
        bool UnblockPlayer(string adminPlayerName, string playerName);

        /// <summary>
        /// Obtiene la lista de jugadores bloqueados.
        /// Solo puede ser ejecutado por un administrador.
        /// </summary>
        /// <param name="adminPlayerName">Nombre del administrador.</param>
        /// <returns>Lista de jugadores bloqueados, o lista vacía si no es admin.</returns>
        IReadOnlyList<string> GetBlockedPlayers(string adminPlayerName);

        /// <summary>
        /// Obtiene el número de jugadores bloqueados.
        /// </summary>
        /// <returns>Cantidad de jugadores bloqueados.</returns>
        int GetBlockedPlayersCount();
    }
}
