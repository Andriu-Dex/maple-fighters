using System;
using Scripts.Core.Domain.Interfaces;

namespace Scripts.Services.PlayerLoginApi
{
    /// <summary>
    /// Resultado del intento de login.
    /// </summary>
    public enum LoginResult
    {
        /// <summary>
        /// Login exitoso.
        /// </summary>
        Success,

        /// <summary>
        /// El jugador no existe (es nuevo).
        /// </summary>
        PlayerNotFound,

        /// <summary>
        /// Contraseña incorrecta.
        /// </summary>
        WrongPassword,

        /// <summary>
        /// Jugador bloqueado por intentos fallidos.
        /// </summary>
        Blocked,

        /// <summary>
        /// Nombre de jugador inválido.
        /// </summary>
        InvalidName,

        /// <summary>
        /// Contraseña inválida (formato).
        /// </summary>
        InvalidPassword,

        /// <summary>
        /// Email con formato inválido.
        /// </summary>
        InvalidEmail,

        /// <summary>
        /// El registro no está completo.
        /// </summary>
        RegistrationIncomplete,

        /// <summary>
        /// Error desconocido.
        /// </summary>
        Error
    }

    /// <summary>
    /// Resultado del registro de jugador.
    /// </summary>
    public enum RegisterResult
    {
        /// <summary>
        /// Registro exitoso.
        /// </summary>
        Success,

        /// <summary>
        /// El nombre ya está en uso.
        /// </summary>
        NameAlreadyExists,

        /// <summary>
        /// El email ya está registrado.
        /// </summary>
        EmailAlreadyExists,

        /// <summary>
        /// Nombre inválido.
        /// </summary>
        InvalidName,

        /// <summary>
        /// Contraseña inválida.
        /// </summary>
        InvalidPassword,

        /// <summary>
        /// Email inválido.
        /// </summary>
        InvalidEmail,

        /// <summary>
        /// Error desconocido.
        /// </summary>
        Error
    }

    /// <summary>
    /// Resultado de verificación de email.
    /// </summary>
    public enum EmailCheckResult
    {
        /// <summary>
        /// Email existe y el registro está completo (usuario existente).
        /// </summary>
        ExistsComplete,

        /// <summary>
        /// Email existe pero el registro está incompleto (necesita completar).
        /// </summary>
        ExistsIncomplete,

        /// <summary>
        /// Email no existe (usuario nuevo).
        /// </summary>
        NotExists,

        /// <summary>
        /// Formato de email inválido.
        /// </summary>
        InvalidFormat,

        /// <summary>
        /// Error al verificar.
        /// </summary>
        Error
    }

    /// <summary>
    /// Interface para la API de login de jugadores.
    /// Sigue el mismo patrón que IAuthenticatorApi e ICharacterProviderApi.
    /// </summary>
    public interface IPlayerLoginApi
    {
        #region v2 Callbacks (Email-based)

        /// <summary>
        /// Callback cuando se completa la verificación de email.
        /// Parámetros: (resultado, email, datos del jugador si existe)
        /// </summary>
        Action<EmailCheckResult, string, IPlayerCredentials> CheckEmailCallback { get; set; }

        /// <summary>
        /// Callback cuando se completa el login por email.
        /// Parámetros: (resultado, intentos restantes, mensaje, datos del jugador)
        /// </summary>
        Action<LoginResult, int, string, IPlayerCredentials> LoginByEmailCallback { get; set; }

        /// <summary>
        /// Callback cuando se completa el registro con email.
        /// Parámetros: (resultado, mensaje, datos del jugador)
        /// </summary>
        Action<RegisterResult, string, IPlayerCredentials> RegisterWithEmailCallback { get; set; }

        #endregion

        #region v1 Callbacks (Legacy - PlayerName-based)

        /// <summary>
        /// Callback cuando se completa la verificación de existencia de jugador.
        /// Parámetros: (existe, nombre del jugador)
        /// </summary>
        Action<bool, string> CheckPlayerExistsCallback { get; set; }

        /// <summary>
        /// Callback cuando se completa el intento de login.
        /// Parámetros: (resultado, intentos restantes, mensaje)
        /// </summary>
        Action<LoginResult, int, string> LoginCallback { get; set; }

        /// <summary>
        /// Callback cuando se completa el registro de jugador.
        /// Parámetros: (resultado, mensaje)
        /// </summary>
        Action<RegisterResult, string> RegisterCallback { get; set; }

        #endregion

        #region v2 Methods (Email-based)

        /// <summary>
        /// Verifica si un email existe y si el registro está completo.
        /// El resultado se entrega via CheckEmailCallback.
        /// </summary>
        /// <param name="email">Email a verificar.</param>
        void CheckEmail(string email);

        /// <summary>
        /// Intenta hacer login con email y contraseña.
        /// El resultado se entrega via LoginByEmailCallback.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <param name="password">Contraseña.</param>
        void LoginByEmail(string email, string password);

        /// <summary>
        /// Registra un nuevo jugador con email (primera fase).
        /// Solo crea el registro con email, sin nombre/contraseña/clase.
        /// El resultado se entrega via RegisterWithEmailCallback.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        void CreateAccountWithEmail(string email);

        /// <summary>
        /// Completa el registro de un jugador existente.
        /// Agrega nombre, contraseña y clase de personaje.
        /// El resultado se entrega via RegisterWithEmailCallback.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <param name="playerName">Nombre del personaje.</param>
        /// <param name="password">Contraseña.</param>
        /// <param name="characterClass">Clase del personaje.</param>
        void CompleteRegistration(string email, string playerName, string password, string characterClass);

        /// <summary>
        /// Verifica si un email está bloqueado.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <returns>True si está bloqueado.</returns>
        bool IsEmailBlocked(string email);

        /// <summary>
        /// Obtiene los intentos restantes para un email.
        /// </summary>
        /// <param name="email">Email del jugador.</param>
        /// <returns>Intentos restantes antes de bloqueo.</returns>
        int GetRemainingAttemptsByEmail(string email);

        #endregion

        #region v1 Methods (Legacy - PlayerName-based)

        /// <summary>
        /// Verifica si un jugador existe por su nombre.
        /// El resultado se entrega via CheckPlayerExistsCallback.
        /// </summary>
        /// <param name="playerName">Nombre del jugador a verificar.</param>
        void CheckPlayerExists(string playerName);

        /// <summary>
        /// Intenta hacer login con las credenciales dadas.
        /// El resultado se entrega via LoginCallback.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <param name="password">Contraseña.</param>
        void Login(string playerName, string password);

        /// <summary>
        /// Registra un nuevo jugador.
        /// El resultado se entrega via RegisterCallback.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <param name="password">Contraseña.</param>
        void Register(string playerName, string password);

        /// <summary>
        /// Obtiene el número de intentos restantes para un jugador.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>Intentos restantes antes de ser bloqueado.</returns>
        int GetRemainingAttempts(string playerName);

        /// <summary>
        /// Verifica si un jugador está bloqueado.
        /// </summary>
        /// <param name="playerName">Nombre del jugador.</param>
        /// <returns>True si está bloqueado.</returns>
        bool IsPlayerBlocked(string playerName);

        #endregion
    }
}
