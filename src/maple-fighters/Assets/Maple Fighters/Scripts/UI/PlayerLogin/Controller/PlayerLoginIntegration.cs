using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Services;
using Scripts.Services.PlayerLoginApi;
using Scripts.UI.CharacterSelection;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Integrador del sistema de login de jugadores con el flujo del juego.
    /// Este componente conecta PlayerLoginController con CharacterViewController
    /// y el resto del sistema de UI del juego.
    /// 
    /// Configuración en Unity:
    ///   1. Agregar a un GameObject en la escena de Login/Menú principal
    ///   2. Asignar las referencias necesarias en el inspector
    ///   3. El componente manejará automáticamente el flujo de login
    /// </summary>
    public class PlayerLoginIntegration : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField]
        private PlayerLoginController loginController;

        [SerializeField]
        private CharacterViewController characterViewController;

        [Header("Scene Settings")]
        [SerializeField]
        private string characterSelectionScene = "CharacterSelection";

        [SerializeField]
        private bool useSceneTransition = false;

        [Header("Admin Panel")]
        [SerializeField]
        private AdminPanel adminPanel;

        private IPlayerRepository playerRepository;
        private ISaveService saveService;

        private void Awake()
        {
            // Obtener servicios
            ServiceLocator.TryGet(out playerRepository);
            ServiceLocator.TryGet(out saveService);
        }

        private void Start()
        {
            // Suscribirse a eventos del controlador de login
            if (loginController != null)
            {
                loginController.OnLoginSuccess += OnLoginSuccess;
                loginController.OnNewPlayerCreation += OnNewPlayerCreation;
                loginController.OnBackToMenu += OnBackToMenu;
            }

            // Verificar si hay un jugador guardado
            CheckForSavedPlayer();
        }

        private void OnDestroy()
        {
            if (loginController != null)
            {
                loginController.OnLoginSuccess -= OnLoginSuccess;
                loginController.OnNewPlayerCreation -= OnNewPlayerCreation;
                loginController.OnBackToMenu -= OnBackToMenu;
            }
        }

        private void CheckForSavedPlayer()
        {
            if (saveService == null) return;

            var savedPlayer = saveService.GetString("current_player", string.Empty);
            
            if (!string.IsNullOrEmpty(savedPlayer) && playerRepository != null)
            {
                // Verificar si el jugador guardado existe y no está bloqueado
                var player = playerRepository.GetPlayer(savedPlayer);
                if (player != null && !player.IsBlocked)
                {
                    Debug.Log($"[PlayerLoginIntegration] Jugador guardado encontrado: {savedPlayer}");
                    // Opcionalmente, auto-login o mostrar mensaje de bienvenida
                }
            }
        }

        private void OnLoginSuccess(string playerName)
        {
            Debug.Log($"[PlayerLoginIntegration] Login exitoso: {playerName}");

            // Configurar UserMetadata
            SetupUserMetadata(playerName);

            // Configurar panel de admin si el usuario es Admin
            SetupAdminPanel(playerName);

            // Ir a selección de personajes
            ShowCharacterSelection();
        }

        private void OnNewPlayerCreation(string playerName)
        {
            Debug.Log($"[PlayerLoginIntegration] Nuevo jugador, creando personaje: {playerName}");

            // Configurar UserMetadata
            SetupUserMetadata(playerName);

            // Ir a creación de personaje
            ShowCharacterCreation(playerName);
        }

        private void OnBackToMenu()
        {
            Debug.Log("[PlayerLoginIntegration] Volviendo al menú");
            // Implementar según necesidad (mostrar menú principal, etc.)
        }

        private void SetupUserMetadata(string playerName)
        {
            var userMetadata = FindObjectOfType<UserMetadata>();
            if (userMetadata != null)
            {
                userMetadata.IsLoggedIn = true;
                
                // Crear UserData con el ID del jugador
                userMetadata.UserData = new Scripts.Services.AuthenticatorApi.UserData
                {
                    id = playerName.ToLowerInvariant() // Usar nombre como ID
                };

                Debug.Log($"[PlayerLoginIntegration] UserMetadata configurado: {playerName}");
            }
            else
            {
                Debug.LogWarning("[PlayerLoginIntegration] UserMetadata no encontrado en la escena");
            }
        }

        private void SetupAdminPanel(string playerName)
        {
            if (adminPanel != null)
            {
                adminPanel.SetCurrentPlayer(playerName);
            }
        }

        private void ShowCharacterSelection()
        {
            if (useSceneTransition)
            {
                // Cargar escena de selección de personajes
                SceneManager.LoadScene(characterSelectionScene);
            }
            else
            {
                // Mostrar CharacterViewController si está en la misma escena
                if (characterViewController != null)
                {
                    characterViewController.gameObject.SetActive(true);
                    characterViewController.ShowCharacterSelection();
                }
            }
        }

        private void ShowCharacterCreation(string playerName)
        {
            // Similar a ShowCharacterSelection, pero indicando que es nuevo
            if (useSceneTransition)
            {
                // Guardar flag de nuevo jugador para la siguiente escena
                if (saveService != null)
                {
                    saveService.SetBool("is_new_player", true);
                    saveService.Save();
                }
                SceneManager.LoadScene(characterSelectionScene);
            }
            else
            {
                if (characterViewController != null)
                {
                    characterViewController.gameObject.SetActive(true);
                    characterViewController.ShowCharacterSelection();
                    // El CharacterViewController debería detectar que es nuevo y mostrar creación
                }
            }
        }

        /// <summary>
        /// Muestra la ventana de login manualmente.
        /// Útil para llamar desde botones de UI o otros scripts.
        /// </summary>
        public void ShowLogin()
        {
            loginController?.ShowLogin();
        }

        /// <summary>
        /// Cierra sesión del jugador actual.
        /// </summary>
        public void Logout()
        {
            if (saveService != null)
            {
                saveService.SetString("current_player", string.Empty);
                saveService.Save();
            }

            var userMetadata = FindObjectOfType<UserMetadata>();
            if (userMetadata != null)
            {
                userMetadata.IsLoggedIn = false;
                userMetadata.UserData = null;
            }

            // Mostrar login de nuevo
            ShowLogin();

            Debug.Log("[PlayerLoginIntegration] Sesión cerrada");
        }
    }
}
