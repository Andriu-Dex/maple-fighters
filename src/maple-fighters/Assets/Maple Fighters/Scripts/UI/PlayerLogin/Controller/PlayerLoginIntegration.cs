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
    /// En v2, la selección de personaje para nuevos usuarios ocurre dentro
    /// del flujo de login, por lo que este componente principalmente maneja
    /// la transición al juego después del login exitoso.
    /// 
    /// Flujo v2:
    ///   - Usuario nuevo: Email -> Selección clase -> Nombre+Password -> Juego
    ///   - Usuario existente: Email -> Password -> Juego (sin selección de clase)
    /// 
    /// Configuración en Unity:
    ///   1. Agregar a un GameObject en la escena de Login/Menú principal
    ///   2. Asignar las referencias necesarias en el inspector
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
                
                userMetadata.UserData = Scripts.Services.AuthenticatorApi.UserData.Create(
                    playerData?.Id ?? email.ToLowerInvariant(),
                    email,
                    playerName,
                    playerData?.CharacterClass
                );

                Debug.Log($"[PlayerLoginIntegration] UserMetadata configurado: {playerName} ({email})");
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
                // En v2, el personaje ya fue seleccionado durante el login
                if (characterViewController != null)
                {
                    characterViewController.gameObject.SetActive(true);
                    characterViewController.ShowCharacterSelection();
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
        /// </summary>
        public void Logout()
        {
            var userMetadata = FindObjectOfType<UserMetadata>();
            if (userMetadata != null)
            {
                userMetadata.IsLoggedIn = false;
                userMetadata.UserData = null;
            }

            loginController?.Logout();

            Debug.Log("[PlayerLoginIntegration] Sesión cerrada");
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
