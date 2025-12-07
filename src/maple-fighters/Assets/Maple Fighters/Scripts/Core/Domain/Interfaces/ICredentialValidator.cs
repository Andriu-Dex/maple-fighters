namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Resultado de la validación de credenciales.
    /// </summary>
    public enum CredentialValidationResult
    {
        /// <summary>
        /// Credenciales válidas, login exitoso.
        /// </summary>
        Valid,

        /// <summary>
        /// El jugador no existe.
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// Contraseña incorrecta.
        /// </summary>
        InvalidPassword,

        /// <summary>
        /// El jugador está bloqueado por intentos fallidos.
        /// </summary>
        PlayerBlocked,

        /// <summary>
        /// El nombre del jugador no cumple con los requisitos.
        /// </summary>
        InvalidPlayerName,

        /// <summary>
        /// La contraseña no cumple con los requisitos.
        /// </summary>
        InvalidPasswordFormat
    }

    /// <summary>
    /// Interface para validar credenciales de jugadores.
    /// Implementa el patrón Strategy para permitir diferentes estrategias de validación.
    /// </summary>
    public interface ICredentialValidator
    {
        /// <summary>
        /// Valida el formato del nombre de jugador.
        /// </summary>
        /// <param name="playerName">Nombre a validar.</param>
        /// <returns>True si el nombre es válido.</returns>
        bool IsValidPlayerName(string playerName);

        /// <summary>
        /// Valida el formato de la contraseña.
        /// </summary>
        /// <param name="password">Contraseña a validar.</param>
        /// <returns>True si la contraseña cumple los requisitos.</returns>
        bool IsValidPasswordFormat(string password);

        /// <summary>
        /// Valida las credenciales completas de un jugador (nombre + contraseña).
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <param name="password">Contraseña proporcionada.</param>
        /// <returns>Resultado de la validación.</returns>
        CredentialValidationResult ValidateCredentials(string playerName, string password);

        /// <summary>
        /// Mensaje de error descriptivo para el resultado de validación.
        /// </summary>
        /// <param name="result">Resultado de la validación.</param>
        /// <returns>Mensaje legible para mostrar al usuario.</returns>
        string GetValidationMessage(CredentialValidationResult result);
    }
}
