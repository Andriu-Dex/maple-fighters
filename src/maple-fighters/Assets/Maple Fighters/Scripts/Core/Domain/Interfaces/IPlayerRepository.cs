using System.Collections.Generic;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Repository para gestionar el almacenamiento y recuperación de jugadores.
    /// Sigue el patrón Repository para abstraer la persistencia de datos.
    /// </summary>
    public interface IPlayerRepository
    {
        #region Email-based Methods (v2)

        /// <summary>
        /// Verifica si existe un jugador con el email especificado.
        /// </summary>
        /// <param name="email">Email del jugador a buscar.</param>
        /// <returns>True si el email existe, false en caso contrario.</returns>
        bool EmailExists(string email);

        /// <summary>
        /// Obtiene las credenciales de un jugador por su email.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <returns>Las credenciales del jugador o null si no existe.</returns>
        IPlayerCredentials GetPlayerByEmail(string email);

        /// <summary>
        /// Registra un nuevo jugador solo con email (registro inicial).
        /// </summary>
        /// <param name="email">Email único del jugador.</param>
        /// <returns>Las credenciales del jugador recién creado o null si el email ya existe.</returns>
        IPlayerCredentials CreatePlayerWithEmail(string email);

        /// <summary>
        /// Registra un nuevo jugador con email y contraseña.
        /// El nombre y clase se asignarán después en CharacterViewController.
        /// </summary>
        /// <param name="email">Email único del jugador.</param>
        /// <param name="password">Contraseña del jugador.</param>
        /// <returns>Las credenciales del jugador recién creado o null si el email ya existe.</returns>
        IPlayerCredentials CreatePlayerWithEmailAndPassword(string email, string password);

        /// <summary>
        /// Completa el registro de un jugador agregando nombre, contraseña y clase de personaje.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <param name="playerName">Nombre del personaje.</param>
        /// <param name="password">Contraseña.</param>
        /// <param name="characterClass">Clase del personaje.</param>
        /// <returns>True si la actualización fue exitosa.</returns>
        bool CompleteRegistration(string email, string playerName, string password, string characterClass);

        /// <summary>
        /// Actualiza la fecha de último login de un jugador.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        void UpdateLastLogin(string email);

        #endregion

        #region PlayerName-based Methods (legacy v1)

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
        /// Actualiza la contraseña de un jugador existente por email.
        /// Útil para usuarios que existen pero no tienen contraseña establecida.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <param name="newPassword">Nueva contraseña.</param>
        /// <returns>True si la actualización fue exitosa.</returns>
        bool UpdatePasswordByEmail(string email, string newPassword);

        #endregion

        #region Login Attempt Tracking

        /// <summary>
        /// Incrementa el contador de intentos fallidos de un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>El nuevo número de intentos fallidos.</returns>
        int IncrementFailedAttempts(string playerName);

        /// <summary>
        /// Incrementa el contador de intentos fallidos de un jugador por email.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <returns>El nuevo número de intentos fallidos.</returns>
        int IncrementFailedAttemptsByEmail(string email);

        /// <summary>
        /// Reinicia el contador de intentos fallidos de un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        void ResetFailedAttempts(string playerName);

        /// <summary>
        /// Reinicia el contador de intentos fallidos de un jugador por email.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        void ResetFailedAttemptsByEmail(string email);

        #endregion

        #region Blocking

        /// <summary>
        /// Bloquea a un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador a bloquear.</param>
        void BlockPlayer(string playerName);

        /// <summary>
        /// Bloquea a un jugador por email.
        /// </summary>
        /// <param name="email">Email del jugador a bloquear.</param>
        void BlockPlayerByEmail(string email);

        /// <summary>
        /// Desbloquea a un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador a desbloquear.</param>
        void UnblockPlayer(string playerName);

        /// <summary>
        /// Desbloquea a un jugador por email.
        /// </summary>
        /// <param name="email">Email del jugador a desbloquear.</param>
        void UnblockPlayerByEmail(string email);

        /// <summary>
        /// Desbloquea a todos los jugadores bloqueados.
        /// </summary>
        void UnblockAllPlayers();

        /// <summary>
        /// Obtiene la lista de todos los jugadores bloqueados.
        /// </summary>
        /// <returns>Lista de nombres de jugadores bloqueados.</returns>
        IReadOnlyList<string> GetBlockedPlayers();

        #endregion
    }
}
