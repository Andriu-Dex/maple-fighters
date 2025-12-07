namespace Scripts.Services.PlayerLoginApi
{
    /// <summary>
    /// Configuración y constantes del sistema de login de jugadores.
    /// </summary>
    public static class PlayerLoginSettings
    {
        /// <summary>
        /// Número máximo de intentos de login antes de bloquear al jugador.
        /// </summary>
        public const int MaxLoginAttempts = 3;

        /// <summary>
        /// Contraseña por defecto para nuevos jugadores (solo desarrollo).
        /// </summary>
        public const string DefaultPassword = "1234";

        /// <summary>
        /// Nombre del jugador administrador.
        /// </summary>
        public const string AdminPlayerName = "Admin";

        /// <summary>
        /// Longitud mínima del nombre de jugador.
        /// </summary>
        public const int MinPlayerNameLength = 3;

        /// <summary>
        /// Longitud máxima del nombre de jugador.
        /// </summary>
        public const int MaxPlayerNameLength = 20;

        /// <summary>
        /// Longitud mínima de la contraseña.
        /// </summary>
        public const int MinPasswordLength = 4;

        /// <summary>
        /// Longitud máxima de la contraseña.
        /// </summary>
        public const int MaxPasswordLength = 20;

        /// <summary>
        /// Clave de almacenamiento para la colección de jugadores.
        /// </summary>
        public const string PlayersStorageKey = "player_credentials";
    }
}
