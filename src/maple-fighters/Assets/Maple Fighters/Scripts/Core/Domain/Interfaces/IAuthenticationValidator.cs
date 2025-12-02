namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para validación de datos de autenticación.
    /// Permite testing y diferentes implementaciones de validación.
    /// </summary>
    public interface IAuthenticationValidator
    {
        /// <summary>
        /// Valida si el email está vacío.
        /// </summary>
        bool IsEmptyEmailAddress(string email, out string message);

        /// <summary>
        /// Valida si el email tiene formato válido.
        /// </summary>
        bool IsInvalidEmailAddress(string email, out string message);

        /// <summary>
        /// Valida si la contraseña está vacía.
        /// </summary>
        bool IsEmptyPassword(string password, out string message);

        /// <summary>
        /// Valida si la contraseña es muy corta.
        /// </summary>
        bool IsPasswordTooShort(string password, out string message);

        /// <summary>
        /// Valida si la confirmación de contraseña está vacía.
        /// </summary>
        bool IsEmptyConfirmPassword(string password, out string message);

        /// <summary>
        /// Valida si la confirmación de contraseña es muy corta.
        /// </summary>
        bool IsConfirmPasswordTooShort(string password, out string message);

        /// <summary>
        /// Valida si las contraseñas coinciden.
        /// </summary>
        bool ArePasswordsDoNotMatch(string password, string confirmPassword, out string message);

        /// <summary>
        /// Valida si el nombre está vacío.
        /// </summary>
        bool IsFirstNameEmpty(string firstName, out string message);

        /// <summary>
        /// Valida si el apellido está vacío.
        /// </summary>
        bool IsLastNameEmpty(string lastName, out string message);

        /// <summary>
        /// Valida si el nombre es muy corto.
        /// </summary>
        bool IsFirstNameTooShort(string firstName, out string message);

        /// <summary>
        /// Valida si el apellido es muy corto.
        /// </summary>
        bool IsLastNameTooShort(string lastName, out string message);

        /// <summary>
        /// Valida el formato del email.
        /// </summary>
        bool IsEmailAddressValid(string emailAddress);

        /// <summary>
        /// Valida los datos de login completos.
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla</param>
        /// <returns>True si los datos son válidos</returns>
        bool ValidateLoginData(string email, string password, out string errorMessage);

        /// <summary>
        /// Valida los datos de registro completos.
        /// </summary>
        /// <param name="email">Email del usuario</param>
        /// <param name="password">Contraseña</param>
        /// <param name="confirmPassword">Confirmación de contraseña</param>
        /// <param name="firstName">Nombre</param>
        /// <param name="lastName">Apellido</param>
        /// <param name="errorMessage">Mensaje de error si la validación falla</param>
        /// <returns>True si los datos son válidos</returns>
        bool ValidateRegistrationData(string email, string password, string confirmPassword, 
            string firstName, string lastName, out string errorMessage);
    }
}
