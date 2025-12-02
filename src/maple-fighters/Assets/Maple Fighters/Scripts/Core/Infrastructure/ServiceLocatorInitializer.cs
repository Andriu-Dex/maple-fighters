using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure.Configuration;
using Scripts.Core.Infrastructure.Factories;
using Scripts.Core.Infrastructure.Persistence;
using Scripts.Core.Infrastructure.Repositories;
using Scripts.Core.Infrastructure.Services;
using Scripts.UI.Authenticator;
using UnityEngine;

namespace Scripts.Core.Infrastructure
{
    /// <summary>
    /// Inicializa el ServiceLocator con los servicios core del juego.
    /// Este componente debe ejecutarse antes que cualquier otro script que use servicios.
    /// 
    /// Configurar en Unity:
    ///   1. Crear un GameObject vacío llamado "Service Locator"
    ///   2. Agregar este componente
    ///   3. En Project Settings > Script Execution Order, poner este script primero (-100)
    ///   O simplemente colocarlo en la primera escena del juego.
    /// </summary>
    public class ServiceLocatorInitializer : MonoBehaviour
    {
        [Header("Persistence Settings")]
        [Tooltip("Tipo de servicio de persistencia a usar")]
        [SerializeField]
        private PersistenceType persistenceType = PersistenceType.Json;

        private static ServiceLocatorInitializer instance;

        private void Awake()
        {
            // Singleton pattern para evitar duplicados entre escenas
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeServices();
        }

        private void InitializeServices()
        {
            if (ServiceLocator.IsInitialized)
            {
                Debug.Log("[ServiceLocatorInitializer] Services already initialized.");
                return;
            }

            // Registrar servicios en orden de dependencia
            RegisterSaveService();
            RegisterInputService();
            RegisterNetworkConfiguration();
            RegisterApiProvider();
            RegisterAuthenticationValidator();
            RegisterEntityServices();

            ServiceLocator.IsInitialized = true;
            Debug.Log("[ServiceLocatorInitializer] All core services initialized.");
        }

        private void RegisterSaveService()
        {
            ISaveService saveService;

            switch (persistenceType)
            {
                case PersistenceType.Json:
                    saveService = new JsonSaveService();
                    break;
                case PersistenceType.PlayerPrefs:
                    saveService = new PlayerPrefsSaveService();
                    break;
                default:
                    saveService = new JsonSaveService();
                    break;
            }

            ServiceLocator.Register<ISaveService>(saveService);
        }

        private void RegisterInputService()
        {
            var inputService = new UnityInputService();
            ServiceLocator.Register<IInputService>(inputService);
        }

        private void RegisterNetworkConfiguration()
        {
            var networkConfig = new NetworkConfigurationAdapter();
            ServiceLocator.Register<INetworkConfiguration>(networkConfig);
        }

        private void RegisterApiProvider()
        {
            var networkConfig = ServiceLocator.Get<INetworkConfiguration>();
            var apiProvider = new ApiProviderService(networkConfig);
            ServiceLocator.Register<IApiProvider>(apiProvider);
        }

        private void RegisterAuthenticationValidator()
        {
            var validator = new AuthenticationValidator();
            ServiceLocator.Register<IAuthenticationValidator>(validator);
        }

        private void RegisterEntityServices()
        {
            var entityRepository = new EntityRepository();
            ServiceLocator.Register<IEntityRepository>(entityRepository);

            var entityFactory = new EntityFactory();
            ServiceLocator.Register<IEntityFactory>(entityFactory);
        }

        private void OnApplicationQuit()
        {
            // Limpiar APIs antes de cerrar
            if (ServiceLocator.TryGet<IApiProvider>(out var apiProvider))
            {
                apiProvider.ClearAll();
            }

            // Guardar datos pendientes antes de cerrar
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                saveService.Save();
                Debug.Log("[ServiceLocatorInitializer] Data saved on application quit.");
            }
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            // Guardar datos cuando la app se pausa (importante en móviles)
            if (pauseStatus && ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                saveService.Save();
            }
        }
    }

    /// <summary>
    /// Tipos de persistencia disponibles.
    /// </summary>
    public enum PersistenceType
    {
        /// <summary>
        /// Guarda datos en archivo JSON en Application.persistentDataPath
        /// </summary>
        Json,
        
        /// <summary>
        /// Usa PlayerPrefs de Unity (registro en Windows, plist en macOS/iOS)
        /// </summary>
        PlayerPrefs
    }
}
