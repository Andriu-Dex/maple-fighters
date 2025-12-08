using Scripts.Core.Domain.Interfaces;
using Scripts.Services;
using Scripts.Services.AuthenticatorApi;
using Scripts.Services.CharacterProviderApi;
using Scripts.Services.ChatApi;
using Scripts.Services.GameApi;
using Scripts.Services.GameProviderApi;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Services
{
    /// <summary>
    /// Implementación de IApiProvider que gestiona las APIs del juego.
    /// Reemplaza la clase estática ApiProvider original.
    /// 
    /// Usa Factory Pattern internamente para crear las APIs correctas
    /// según el entorno de ejecución (Production, Development, Editor).
    /// </summary>
    public class ApiProviderService : IApiProvider
    {
        private readonly INetworkConfiguration networkConfiguration;

        private IAuthenticatorApi authenticatorApi;
        private IGameApi gameApi;
        private IGameProviderApi gameProviderApi;
        private ICharacterProviderApi characterProviderApi;
        private IChatApi chatApi;

        public ApiProviderService(INetworkConfiguration networkConfiguration)
        {
            this.networkConfiguration = networkConfiguration;
        }

        public IAuthenticatorApi GetAuthenticatorApi()
        {
            if (authenticatorApi == null)
            {
                authenticatorApi = CreateAuthenticatorApi();
            }
            return authenticatorApi;
        }

        public IGameApi GetGameApi()
        {
            if (gameApi == null)
            {
                gameApi = CreateGameApi();
            }
            return gameApi;
        }

        public IGameProviderApi GetGameProviderApi()
        {
            if (gameProviderApi == null)
            {
                gameProviderApi = CreateGameProviderApi();
            }
            return gameProviderApi;
        }

        public ICharacterProviderApi GetCharacterProviderApi()
        {
            if (characterProviderApi == null)
            {
                characterProviderApi = CreateCharacterProviderApi();
            }
            return characterProviderApi;
        }

        public IChatApi GetChatApi()
        {
            if (chatApi == null)
            {
                chatApi = CreateChatApi();
            }
            return chatApi;
        }

        public void ClearAuthenticatorApi()
        {
            authenticatorApi = null;
        }

        public void ClearGameApi()
        {
            gameApi = null;
        }

        public void ClearGameProviderApi()
        {
            gameProviderApi = null;
        }

        public void ClearCharacterProviderApi()
        {
            characterProviderApi = null;
        }

        public void ClearChatApi()
        {
            chatApi = null;
        }

        public void ClearAll()
        {
            ClearAuthenticatorApi();
            ClearGameApi();
            ClearGameProviderApi();
            ClearCharacterProviderApi();
            ClearChatApi();
        }

        #region Factory Methods

        private IAuthenticatorApi CreateAuthenticatorApi()
        {
            if (networkConfiguration.ShouldUseRealApis())
            {
                return HttpAuthenticatorApi.GetInstance();
            }
            return DummyAuthenticatorApi.GetInstance();
        }

        private IGameApi CreateGameApi()
        {
            if (networkConfiguration.ShouldUseRealApis())
            {
                return WebSocketGameApi.GetInstance();
            }
            return DummyGameApi.GetInstance();
        }

        private IGameProviderApi CreateGameProviderApi()
        {
            if (networkConfiguration.ShouldUseRealApis())
            {
                return HttpGameProviderApi.GetInstance();
            }
            return DummyGameProviderApi.GetInstance();
        }

        private ICharacterProviderApi CreateCharacterProviderApi()
        {
            // SIEMPRE usar DummyCharacterProviderApi para persistencia local
            // HttpCharacterProviderApi.UpdateCharacter está vacío y no guarda datos
            return DummyCharacterProviderApi.GetInstance();
        }

        private IChatApi CreateChatApi()
        {
            if (networkConfiguration.ShouldUseRealApis())
            {
                return WebSocketChatApi.GetInstance();
            }
            return DummyChatApi.GetInstance();
        }

        #endregion
    }
}
