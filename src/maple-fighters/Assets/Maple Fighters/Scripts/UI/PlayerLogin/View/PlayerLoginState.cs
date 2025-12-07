namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Estados posibles del flujo de login de jugadores.
    /// </summary>
    public enum PlayerLoginState
    {
        /// <summary>
        /// Estado inicial - esperando entrada del nombre.
        /// </summary>
        EnterName,

        /// <summary>
        /// Verificando si el jugador existe.
        /// </summary>
        CheckingName,

        /// <summary>
        /// Jugador existe - esperando contraseña.
        /// </summary>
        EnterPassword,

        /// <summary>
        /// Validando credenciales.
        /// </summary>
        Validating,

        /// <summary>
        /// Login exitoso - entrando al juego.
        /// </summary>
        Success,

        /// <summary>
        /// Jugador no existe - redirigiendo a creación de personaje.
        /// </summary>
        NewPlayer,

        /// <summary>
        /// Jugador bloqueado por intentos fallidos.
        /// </summary>
        Blocked,

        /// <summary>
        /// Error en el proceso de login.
        /// </summary>
        Error
    }
}
