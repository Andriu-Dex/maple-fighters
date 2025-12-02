using ScriptableObjects.Configurations;
using Scripts.Core.Domain.Interfaces;

namespace Scripts.Core.Infrastructure.Configuration
{
    /// <summary>
    /// Adaptador que envuelve el ScriptableObject NetworkConfiguration
    /// e implementa INetworkConfiguration.
    /// Permite usar el ScriptableObject existente a través de una interface.
    /// </summary>
    public class NetworkConfigurationAdapter : INetworkConfiguration
    {
        private readonly NetworkConfiguration networkConfiguration;

        public NetworkConfigurationAdapter()
        {
            networkConfiguration = NetworkConfiguration.GetInstance();
        }

        public NetworkConfigurationAdapter(NetworkConfiguration configuration)
        {
            networkConfiguration = configuration;
        }

        public string GetProtocol()
        {
            return networkConfiguration?.GetProtocol() ?? string.Empty;
        }

        public string GetHost()
        {
            return networkConfiguration?.GetHost() ?? string.Empty;
        }

        public bool IsEditor()
        {
            return networkConfiguration?.IsEditor() ?? true;
        }

        public bool IsDevelopment()
        {
            return networkConfiguration?.IsDevelopment() ?? false;
        }

        public bool IsProduction()
        {
            return networkConfiguration?.IsProduction() ?? false;
        }

        public bool ShouldUseRealApis()
        {
            return IsProduction() || IsDevelopment();
        }
    }
}
