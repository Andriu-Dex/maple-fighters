namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para la configuración de red del juego.
    /// Abstrae el acceso a la configuración de hosting y entorno.
    /// </summary>
    public interface INetworkConfiguration
    {
        /// <summary>
        /// Obtiene el protocolo de conexión (http, https, ws, wss).
        /// </summary>
        string GetProtocol();

        /// <summary>
        /// Obtiene el host del servidor.
        /// </summary>
        string GetHost();

        /// <summary>
        /// Indica si está en modo Editor (para pruebas locales).
        /// </summary>
        bool IsEditor();

        /// <summary>
        /// Indica si está en modo Development.
        /// </summary>
        bool IsDevelopment();

        /// <summary>
        /// Indica si está en modo Production.
        /// </summary>
        bool IsProduction();

        /// <summary>
        /// Indica si debe usar APIs reales (Production o Development).
        /// </summary>
        bool ShouldUseRealApis();
    }
}
