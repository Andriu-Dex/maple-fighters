using ScriptableObjects.Configurations;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Services.AuthenticatorApi;
using Scripts.Services.CharacterProviderApi;
using Scripts.Services.ChatApi;
using Scripts.Services.GameApi;
using Scripts.Services.GameProviderApi;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.Services
{
    /// <summary>
    /// Proveedor de APIs del juego.
    /// 
    /// NOTA: Esta clase ahora actúa como fachada estática para mantener
    /// compatibilidad con el código existente. Internamente delega al
    /// IApiProvider registrado en ServiceLocator cuando está disponible.
    /// 
    /// Para nuevo código, se recomienda usar:
    ///   var apiProvider = ServiceLocator.Get&lt;IApiProvider&gt;();
    /// </summary>
    public static class ApiProvider
    {
        #region AuthenticatorApi
        public static IAuthenticatorApi ProvideAuthenticatorApi()
        {
            // Intentar usar el nuevo sistema si está disponible
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                return apiProvider.GetAuthenticatorApi();
            }

            // Fallback al comportamiento original
            var networkConfiguration = NetworkConfiguration.GetInstance();
            if (networkConfiguration.IsProduction() ||
                networkConfiguration.IsDevelopment())
            {
                authenticatorApi = HttpAuthenticatorApi.GetInstance();
            }
            else
            {
                authenticatorApi = DummyAuthenticatorApi.GetInstance();
            }

            return authenticatorApi;
        }

        public static void RemoveAuthenticatorApi()
        {
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                apiProvider.ClearAuthenticatorApi();
                return;
            }
            authenticatorApi = null;
        }

        private static IAuthenticatorApi authenticatorApi;
        #endregion

        #region GameApi
        public static IGameApi ProvideGameApi()
        {
            // Intentar usar el nuevo sistema si está disponible
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                return apiProvider.GetGameApi();
            }

            // Fallback al comportamiento original
            var networkConfiguration = NetworkConfiguration.GetInstance();
            if (networkConfiguration.IsProduction() ||
                networkConfiguration.IsDevelopment())
            {
                gameApi = WebSocketGameApi.GetInstance();
            }
            else
            {
                gameApi = DummyGameApi.GetInstance();
            }

            return gameApi;
        }

        public static void RemoveGameApiProvider()
        {
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                apiProvider.ClearGameApi();
                return;
            }
            gameApi = null;
        }

        private static IGameApi gameApi;
        #endregion

        #region GameProviderApi
        public static IGameProviderApi ProvideGameProviderApi()
        {
            // Intentar usar el nuevo sistema si está disponible
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                return apiProvider.GetGameProviderApi();
            }

            // Fallback al comportamiento original
            var networkConfiguration = NetworkConfiguration.GetInstance();
            if (networkConfiguration.IsProduction() ||
                networkConfiguration.IsDevelopment())
            {
                gameProviderApi = HttpGameProviderApi.GetInstance();
            }
            else
            {
                gameProviderApi = DummyGameProviderApi.GetInstance();
            }

            return gameProviderApi;
        }

        public static void RemoveGameProviderApi()
        {
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                apiProvider.ClearGameProviderApi();
                return;
            }
            gameProviderApi = null;
        }

        private static IGameProviderApi gameProviderApi;
        #endregion

        #region CharacterProviderApi
        public static ICharacterProviderApi ProvideCharacterProviderApi()
        {
            // Intentar usar el nuevo sistema si está disponible
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                return apiProvider.GetCharacterProviderApi();
            }

            // SIEMPRE usar DummyCharacterProviderApi para persistencia local
            // HttpCharacterProviderApi.UpdateCharacter está vacío y no guarda datos
            if (characterProviderApi == null)
            {
                characterProviderApi = DummyCharacterProviderApi.GetInstance();
            }

            return characterProviderApi;
        }

        public static void RemoveCharacterProviderApi()
        {
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                apiProvider.ClearCharacterProviderApi();
                return;
            }
            characterProviderApi = null;
        }

        private static ICharacterProviderApi characterProviderApi;
        #endregion

        #region ChatApi
        public static IChatApi ProvideChatApi()
        {
            // Intentar usar el nuevo sistema si está disponible
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                return apiProvider.GetChatApi();
            }

            // Fallback al comportamiento original
            var networkConfiguration = NetworkConfiguration.GetInstance();
            if (networkConfiguration.IsProduction() ||
                networkConfiguration.IsDevelopment())
            {
                chatApi = WebSocketChatApi.GetInstance();
            }
            else
            {
                chatApi = DummyChatApi.GetInstance();
            }

            return chatApi;
        }

        public static void RemoveChatApiProvider()
        {
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                apiProvider.ClearChatApi();
                return;
            }
            chatApi = null;
        }

        private static IChatApi chatApi;
        #endregion

        #region PlayerLoginApi
        /// <summary>
        /// Provee la API de login de jugadores.
        /// </summary>
        public static IPlayerLoginApi ProvidePlayerLoginApi()
        {
            if (playerLoginApi == null)
            {
                playerLoginApi = DummyPlayerLoginApi.GetInstance();
            }
            return playerLoginApi;
        }

        /// <summary>
        /// Elimina la referencia a la API de login de jugadores.
        /// </summary>
        public static void RemovePlayerLoginApi()
        {
            playerLoginApi = null;
        }

        private static IPlayerLoginApi playerLoginApi;
        #endregion
    }
}