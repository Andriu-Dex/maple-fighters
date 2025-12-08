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
    /// Integrador del sistema de login de jugadores v2 con el flujo del juego.
    /// 
    /// Flujo v2 simplificado:
    ///   - Usuario nuevo: Email -> Crear cuenta -> CharacterViewController (crear personaje) -> Juego
    ///   - Usuario existente: Email -> Password -> CharacterViewController (elegir personaje) -> Juego
    /// 
    /// El CharacterViewController maneja toda la lógica de crear/elegir/eliminar personajes.
    /// 
    /// Configuración en Unity:
    ///   1. Agregar a un GameObject en la escena de Login/Menú principal
    ///   2. Asignar las referencias necesarias en el inspector
    ///   3. Asignar el CharacterViewController de la escena
    /// </summary>
    public class PlayerLoginIntegration : MonoBehaviour
    {
        [Header("Controllers")]
        [SerializeField]
        private PlayerLoginController loginController;

        [SerializeField]
        private CharacterViewController characterViewController;

        [Header("Scene Settings")]
        [Tooltip("Escena a cargar después del login exitoso (si useSceneTransition=true)")]
        [SerializeField]
        private string gameScene = "Game";

        [SerializeField]
        private bool useSceneTransition = false;

        [Header("Admin Panel")]
        [SerializeField]
        private AdminPanel adminPanel;

        private IPlayerRepository playerRepository;
        private ISaveService saveService;

        private void Awake()
        {
            ServiceLocator.TryGet(out playerRepository);
            ServiceLocator.TryGet(out saveService);
        }

        private void Start()
        {
            if (loginController != null)
            {
                loginController.OnLoginSuccess += OnLoginSuccess;
                loginController.OnBackToMenu += OnBackToMenu;
            }
        }

        private void OnDestroy()
        {
            if (loginController != null)
            {
                loginController.OnLoginSuccess -= OnLoginSuccess;
                loginController.OnBackToMenu -= OnBackToMenu;
            }
        }

        private void OnLoginSuccess(string email, string playerName, IPlayerCredentials playerData)
        {
            Debug.Log($"[PlayerLoginIntegration] Login v2 exitoso: {email} -> {playerName} (Clase: {playerData?.CharacterClass})");

            // Configurar UserMetadata con datos completos
            SetupUserMetadata(email, playerName, playerData);

            // Configurar panel de admin si corresponde
            SetupAdminPanel(playerName);

            // Ir al juego (en v2 ya se seleccionó el personaje durante el login)
            StartGame();
        }

        private void OnBackToMenu()
        {
            Debug.Log("[PlayerLoginIntegration] Volviendo al menú");
            // Implementar según necesidad
        }

        private void SetupUserMetadata(string email, string playerName, IPlayerCredentials playerData)
        {
            var userMetadata = FindObjectOfType<UserMetadata>();
            if (userMetadata != null)
            {
                userMetadata.IsLoggedIn = true;
                
                // FASE 1: Establecer el userId desde las credenciales del login
                // Esto vincula los personajes a la cuenta del usuario (no a un ID aleatorio)
                var userId = playerData?.Id ?? email.ToLowerInvariant();
                userMetadata.SetUserIdFromCredentials(userId);
                
                // Configurar datos adicionales del usuario
                userMetadata.UserData = Scripts.Services.AuthenticatorApi.UserData.Create(
                    userId,
                    email,
                    playerName,
                    playerData?.CharacterClass
                );

                Debug.Log($"[PlayerLoginIntegration] UserMetadata configurado con userId: {userId} para {playerName} ({email})");
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

        private void StartGame()
        {
            if (useSceneTransition)
            {
                // Cargar escena del juego
                SceneManager.LoadScene(gameScene);
            }
            else
            {
                // Activar CharacterViewController si está en la misma escena
                // FASE 3: Usar flujo inteligente que dirige automáticamente
                // - Sin personaje -> Crear personaje
                // - Con personaje -> Ir al juego
                if (characterViewController != null)
                {
                    characterViewController.gameObject.SetActive(true);
                    characterViewController.ShowCharacterSelectionSmart();
                }
            }
        }

        /// <summary>
        /// Muestra la ventana de login manualmente.
        /// </summary>
        public void ShowLogin()
        {
            loginController?.ShowLogin();
        }

        /// <summary>
        /// Fuerza mostrar login sin auto-login.
        /// </summary>
        public void ShowLoginForced()
        {
            loginController?.ShowLoginForced();
        }

        /// <summary>
        /// Cierra sesión del jugador actual y muestra login.
        /// Limpia el userId para que el siguiente usuario no vea datos anteriores.
        /// </summary>
        public void Logout()
        {
            var userMetadata = FindObjectOfType<UserMetadata>();
            if (userMetadata != null)
            {
                // Usar ClearSession() para limpiar completamente la sesión
                userMetadata.ClearSession();
            }

            loginController?.Logout();

            Debug.Log("[PlayerLoginIntegration] Sesión cerrada y datos de usuario limpiados");
        }

        /// <summary>
        /// Email del jugador actual (después del login).
        /// </summary>
        public string CurrentEmail => loginController?.CurrentEmail;

        /// <summary>
        /// Nombre del jugador actual (después del login).
        /// </summary>
        public string CurrentPlayerName => loginController?.CurrentPlayerName;

        /// <summary>
        /// Datos completos del jugador actual.
        /// </summary>
        public IPlayerCredentials CurrentPlayerData => loginController?.CurrentPlayerData;
    }
}
