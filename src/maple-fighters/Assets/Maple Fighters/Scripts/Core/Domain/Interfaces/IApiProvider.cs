using Scripts.Services.AuthenticatorApi;
using Scripts.Services.CharacterProviderApi;
using Scripts.Services.ChatApi;
using Scripts.Services.GameApi;
using Scripts.Services.GameProviderApi;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para el proveedor de APIs del juego.
    /// Abstrae la creación y acceso a las diferentes APIs de red.
    /// </summary>
    public interface IApiProvider
    {
        /// <summary>
        /// Obtiene la API de autenticación.
        /// </summary>
        IAuthenticatorApi GetAuthenticatorApi();

        /// <summary>
        /// Obtiene la API del juego (WebSocket para gameplay).
        /// </summary>
        IGameApi GetGameApi();

        /// <summary>
        /// Obtiene la API del proveedor de juegos (lista de servidores).
        /// </summary>
        IGameProviderApi GetGameProviderApi();

        /// <summary>
        /// Obtiene la API del proveedor de personajes.
        /// </summary>
        ICharacterProviderApi GetCharacterProviderApi();

        /// <summary>
        /// Obtiene la API del chat.
        /// </summary>
        IChatApi GetChatApi();

        /// <summary>
        /// Limpia la referencia a la API de autenticación.
        /// </summary>
        void ClearAuthenticatorApi();

        /// <summary>
        /// Limpia la referencia a la API del juego.
        /// </summary>
        void ClearGameApi();

        /// <summary>
        /// Limpia la referencia a la API del proveedor de juegos.
        /// </summary>
        void ClearGameProviderApi();

        /// <summary>
        /// Limpia la referencia a la API del proveedor de personajes.
        /// </summary>
        void ClearCharacterProviderApi();

        /// <summary>
        /// Limpia la referencia a la API del chat.
        /// </summary>
        void ClearChatApi();

        /// <summary>
        /// Limpia todas las referencias a APIs.
        /// </summary>
        void ClearAll();
    }
}
