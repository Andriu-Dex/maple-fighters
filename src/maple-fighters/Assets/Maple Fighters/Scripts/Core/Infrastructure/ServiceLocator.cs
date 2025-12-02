using System;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Core.Infrastructure
{
    /// <summary>
    /// Service Locator simple para registrar y obtener servicios.
    /// Evita el uso excesivo de FindObjectOfType y Singletons estáticos.
    /// 
    /// Uso:
    ///   // Registrar un servicio
    ///   ServiceLocator.Register<ISaveService>(new JsonSaveService());
    ///   
    ///   // Obtener un servicio
    ///   var saveService = ServiceLocator.Get<ISaveService>();
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> services = new Dictionary<Type, object>();
        private static bool isInitialized = false;

        /// <summary>
        /// Registra un servicio con su interfaz.
        /// </summary>
        /// <typeparam name="T">Tipo de la interfaz del servicio.</typeparam>
        /// <param name="service">Instancia del servicio.</param>
        public static void Register<T>(T service) where T : class
        {
            var type = typeof(T);
            
            if (services.ContainsKey(type))
            {
                Debug.LogWarning($"[ServiceLocator] Service {type.Name} is already registered. Replacing...");
                services[type] = service;
            }
            else
            {
                services.Add(type, service);
                Debug.Log($"[ServiceLocator] Registered service: {type.Name}");
            }
        }

        /// <summary>
        /// Obtiene un servicio registrado.
        /// </summary>
        /// <typeparam name="T">Tipo de la interfaz del servicio.</typeparam>
        /// <returns>Instancia del servicio o null si no está registrado.</returns>
        public static T Get<T>() where T : class
        {
            var type = typeof(T);
            
            if (services.TryGetValue(type, out var service))
            {
                return service as T;
            }

            Debug.LogWarning($"[ServiceLocator] Service {type.Name} is not registered.");
            return null;
        }

        /// <summary>
        /// Intenta obtener un servicio registrado.
        /// </summary>
        /// <typeparam name="T">Tipo de la interfaz del servicio.</typeparam>
        /// <param name="service">Servicio encontrado.</param>
        /// <returns>True si el servicio existe.</returns>
        public static bool TryGet<T>(out T service) where T : class
        {
            var type = typeof(T);
            
            if (services.TryGetValue(type, out var foundService))
            {
                service = foundService as T;
                return service != null;
            }

            service = null;
            return false;
        }

        /// <summary>
        /// Verifica si un servicio está registrado.
        /// </summary>
        /// <typeparam name="T">Tipo de la interfaz del servicio.</typeparam>
        /// <returns>True si el servicio está registrado.</returns>
        public static bool IsRegistered<T>() where T : class
        {
            return services.ContainsKey(typeof(T));
        }

        /// <summary>
        /// Elimina un servicio del registro.
        /// </summary>
        /// <typeparam name="T">Tipo de la interfaz del servicio.</typeparam>
        public static void Unregister<T>() where T : class
        {
            var type = typeof(T);
            
            if (services.Remove(type))
            {
                Debug.Log($"[ServiceLocator] Unregistered service: {type.Name}");
            }
        }

        /// <summary>
        /// Limpia todos los servicios registrados.
        /// Útil para testing o cambio de escena.
        /// </summary>
        public static void Clear()
        {
            services.Clear();
            isInitialized = false;
            Debug.Log("[ServiceLocator] All services cleared.");
        }

        /// <summary>
        /// Indica si el ServiceLocator ha sido inicializado.
        /// </summary>
        public static bool IsInitialized
        {
            get => isInitialized;
            set => isInitialized = value;
        }
    }
}
