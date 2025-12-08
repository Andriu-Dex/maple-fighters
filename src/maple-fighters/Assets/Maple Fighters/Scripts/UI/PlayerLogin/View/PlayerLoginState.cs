namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Estados posibles del flujo de login de jugadores (v2 con email).
    /// </summary>
    public enum PlayerLoginState
    {
        #region v2 States (Email-based flow)

        /// <summary>
        /// Estado inicial - esperando entrada del email.
        /// </summary>
        EnterEmail,

        /// <summary>
        /// Verificando si el email existe.
        /// </summary>
        CheckingEmail,

        /// <summary>
        /// Email existe y registro completo - pedir contraseña para login.
        /// </summary>
        EnterPasswordForLogin,

        /// <summary>
        /// Email no existe - es nuevo usuario, seleccionar personaje.
        /// </summary>
        SelectCharacter,

        /// <summary>
        /// Personaje seleccionado - pedir nombre y contraseña para completar registro.
        /// </summary>
        EnterRegistrationData,

        /// <summary>
        /// Validando credenciales (login o registro).
        /// </summary>
        Validating,

        /// <summary>
        /// Login/Registro exitoso - entrando al juego.
        /// </summary>
        Success,

        /// <summary>
        /// Cuenta bloqueada por intentos fallidos.
        /// </summary>
        Blocked,

        /// <summary>
        /// Error en el proceso.
        /// </summary>
        Error,

        #endregion

        #region v1 States (Legacy - PlayerName-based)

        /// <summary>
        /// [Legacy v1] Estado inicial - esperando entrada del nombre.
        /// </summary>
        EnterName,

        /// <summary>
        /// [Legacy v1] Verificando si el jugador existe.
        /// </summary>
        CheckingName,

        /// <summary>
        /// [Legacy v1] Jugador existe - esperando contraseña.
        /// </summary>
        EnterPassword,

        /// <summary>
        /// [Legacy v1] Jugador no existe - redirigiendo a creación.
        /// </summary>
        NewPlayer

        #endregion
    }
}
