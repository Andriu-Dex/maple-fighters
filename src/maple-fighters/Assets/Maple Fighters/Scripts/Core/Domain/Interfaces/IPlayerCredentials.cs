namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface que define los datos de credenciales de un jugador.
    /// Representa la información necesaria para autenticar a un jugador.
    /// </summary>
    public interface IPlayerCredentials
    {
        /// <summary>
        /// Nombre único del jugador (usado como identificador).
        /// </summary>
        string PlayerName { get; }

        /// <summary>
        /// Contraseña del jugador.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Indica si el jugador está bloqueado por intentos fallidos.
        /// </summary>
        bool IsBlocked { get; }

        /// <summary>
        /// Número de intentos de login fallidos consecutivos.
        /// </summary>
        int FailedAttempts { get; }
    }
}
