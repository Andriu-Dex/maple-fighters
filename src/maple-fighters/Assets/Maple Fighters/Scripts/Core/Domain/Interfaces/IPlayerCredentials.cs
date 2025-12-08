using System;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface que define los datos de credenciales de un jugador.
    /// Representa la información necesaria para autenticar a un jugador.
    /// </summary>
    public interface IPlayerCredentials
    {
        /// <summary>
        /// Identificador único del jugador (GUID).
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Email único del jugador (usado para login).
        /// </summary>
        string Email { get; }

        /// <summary>
        /// Nombre del personaje del jugador.
        /// </summary>
        string PlayerName { get; }

        /// <summary>
        /// Contraseña del jugador.
        /// </summary>
        string Password { get; }

        /// <summary>
        /// Clase del personaje seleccionado (e.g., "Knight", "Archer", "Wizard").
        /// </summary>
        string CharacterClass { get; }

        /// <summary>
        /// ID único del personaje para el servidor.
        /// </summary>
        string CharacterId { get; }

        /// <summary>
        /// Indica si el jugador está bloqueado por intentos fallidos.
        /// </summary>
        bool IsBlocked { get; }

        /// <summary>
        /// Número de intentos de login fallidos consecutivos.
        /// </summary>
        int FailedAttempts { get; }

        /// <summary>
        /// Fecha y hora de creación de la cuenta.
        /// </summary>
        DateTime CreatedAt { get; }

        /// <summary>
        /// Fecha y hora del último login exitoso.
        /// </summary>
        DateTime LastLoginAt { get; }

        /// <summary>
        /// Indica si el registro está completo (tiene nombre y contraseña).
        /// </summary>
        bool IsRegistrationComplete();

        /// <summary>
        /// Indica si el jugador tiene una clase de personaje seleccionada.
        /// </summary>
        bool HasCharacter();
    }
}
