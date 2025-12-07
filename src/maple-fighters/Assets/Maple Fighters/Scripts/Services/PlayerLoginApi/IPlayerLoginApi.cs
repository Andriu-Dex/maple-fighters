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
        /// Nombre inválido.
        /// </summary>
        InvalidName,

        /// <summary>
        /// Contraseña inválida.
        /// </summary>
        InvalidPassword,

        /// <summary>
        /// Error desconocido.
        /// </summary>
        Error
    }

    /// <summary>
    /// Interface para la API de login de jugadores.
    /// Sigue el mismo patrón que IAuthenticatorApi e ICharacterProviderApi.
    /// </summary>
    public interface IPlayerLoginApi
    {
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
    }
}
