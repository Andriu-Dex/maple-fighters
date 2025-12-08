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
        InvalidPasswordFormat,

        /// <summary>
        /// El formato del email es inválido.
        /// </summary>
        InvalidEmailFormat,

        /// <summary>
        /// El email ya está registrado.
        /// </summary>
        EmailAlreadyExists,

        /// <summary>
        /// El nombre de jugador ya está en uso.
        /// </summary>
        PlayerNameAlreadyExists,

        /// <summary>
        /// El registro no está completo (falta nombre/contraseña/clase).
        /// </summary>
        RegistrationIncomplete
    }

    /// <summary>
    /// Interface para validar credenciales de jugadores.
    /// Implementa el patrón Strategy para permitir diferentes estrategias de validación.
    /// </summary>
    public interface ICredentialValidator
    {
        #region Email Validation (v2)

        /// <summary>
        /// Valida el formato del email.
        /// </summary>
        /// <param name="email">Email a validar.</param>
        /// <returns>True si el email tiene un formato válido.</returns>
        bool IsValidEmail(string email);

        /// <summary>
        /// Valida las credenciales usando email y contraseña.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <param name="password">Contraseña proporcionada.</param>
        /// <returns>Resultado de la validación.</returns>
        CredentialValidationResult ValidateCredentialsByEmail(string email, string password);

        /// <summary>
        /// Valida si un email puede ser usado para registro.
        /// </summary>
        /// <param name="email">Email a validar.</param>
        /// <returns>Resultado de la validación.</returns>
        CredentialValidationResult ValidateEmailForRegistration(string email);

        /// <summary>
        /// Valida los datos de registro completo.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <param name="playerName">Nombre del personaje.</param>
        /// <param name="password">Contraseña.</param>
        /// <param name="characterClass">Clase del personaje.</param>
        /// <returns>Resultado de la validación.</returns>
        CredentialValidationResult ValidateRegistrationData(string email, string playerName, string password, string characterClass);

        #endregion

        #region PlayerName Validation (legacy v1)

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

        #endregion

        /// <summary>
        /// Mensaje de error descriptivo para el resultado de validación.
        /// </summary>
        /// <param name="result">Resultado de la validación.</param>
        /// <returns>Mensaje legible para mostrar al usuario.</returns>
        string GetValidationMessage(CredentialValidationResult result);
    }
}
