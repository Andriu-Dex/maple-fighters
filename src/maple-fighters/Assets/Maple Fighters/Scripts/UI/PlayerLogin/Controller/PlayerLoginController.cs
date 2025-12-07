using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Services;
using Scripts.Services.PlayerLoginApi;
using UI;
using UnityEngine;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Controlador principal del flujo de login de jugadores.
    /// Coordina la vista, el presenter y los servicios del juego.
    /// Usa lazy initialization para evitar problemas de orden de ejecución.
    /// Si no se asigna LoginWindow, se crea automáticamente desde Resources.
    /// </summary>
    public class PlayerLoginController : MonoBehaviour
    {
        [Header("View Reference (Opcional)")]
        [Tooltip("Si no se asigna, se crea automáticamente desde Resources/UI/PlayerLoginWindow")]
        [SerializeField]
        private PlayerLoginWindow loginWindow;

        [Header("Settings")]
        [SerializeField]
        private bool showOnStart = true;

        private PlayerLoginPresenter presenter;
        private IPlayerLoginApi cachedLoginApi;
        private ICredentialValidator cachedValidator;
        private bool servicesInitialized;
        private bool windowCreatedDynamically;

        /// <summary>
        /// Se dispara cuando el login es exitoso y el jugador puede entrar al juego.
        /// Parámetro: nombre del jugador.
        /// </summary>
        public event Action<string> OnLoginSuccess;

        /// <summary>
        /// Se dispara cuando un nuevo jugador necesita crear su personaje.
        /// Parámetro: nombre del jugador.
        /// </summary>
        public event Action<string> OnNewPlayerCreation;

        /// <summary>
        /// Se dispara cuando el usuario quiere volver (salir del login).
        /// </summary>
        public event Action OnBackToMenu;

        private void Start()
        {
            // Inicializar en Start() para dar tiempo a ServiceLocatorInitializer
            InitializeServices();
            
            // Crear ventana si no está asignada
            if (loginWindow == null)
            {
                CreateLoginWindow();
            }

            InitializePresenter();

            if (showOnStart && loginWindow != null)
            {
                ShowLogin();
            }
        }

        private void OnDestroy()
        {
            CleanupPresenter();
            
            // Destruir ventana si fue creada dinámicamente
            if (windowCreatedDynamically && loginWindow != null)
            {
                Destroy(loginWindow.gameObject);
            }
        }

        private void CreateLoginWindow()
        {
            // Crear usando UICreator (mismo patrón que CharacterViewController)
            loginWindow = UICreator
                .GetInstance()
                .Create<PlayerLoginWindow>(UICanvasLayer.Foreground, UIIndex.End);

            if (loginWindow != null)
            {
                windowCreatedDynamically = true;
                Debug.Log("[PlayerLoginController] PlayerLoginWindow creada desde Resources");
            }
            else
            {
                Debug.LogError("[PlayerLoginController] No se pudo crear PlayerLoginWindow. Verifica que existe el prefab en Resources/UI/PlayerLoginWindow");
            }
        }

        private void InitializeServices()
        {
            if (servicesInitialized) return;

            // Obtener API de login
            cachedLoginApi = ApiProvider.ProvidePlayerLoginApi();

            // Obtener validador del ServiceLocator
            ServiceLocator.TryGet(out cachedValidator);

            if (cachedLoginApi == null)
            {
                Debug.LogWarning("[PlayerLoginController] No se pudo obtener IPlayerLoginApi");
            }

            if (cachedValidator == null)
            {
                Debug.LogWarning("[PlayerLoginController] No se pudo obtener ICredentialValidator. ¿ServiceLocatorInitializer está en la escena?");
            }

            servicesInitialized = true;
        }

        private void InitializePresenter()
        {
            if (loginWindow == null)
            {
                Debug.LogError("[PlayerLoginController] LoginWindow no disponible");
                return;
            }

            presenter = new PlayerLoginPresenter(loginWindow, cachedLoginApi, cachedValidator);
            presenter.LoginSuccessful += OnPresenterLoginSuccessful;
            presenter.NewPlayerRequested += OnPresenterNewPlayerRequested;
            presenter.BackRequested += OnPresenterBackRequested;
        }

        private void CleanupPresenter()
        {
            if (presenter != null)
            {
                presenter.LoginSuccessful -= OnPresenterLoginSuccessful;
                presenter.NewPlayerRequested -= OnPresenterNewPlayerRequested;
                presenter.BackRequested -= OnPresenterBackRequested;
                presenter.Dispose();
                presenter = null;
            }
        }

        /// <summary>
        /// Muestra la ventana de login.
        /// </summary>
        public void ShowLogin()
        {
            if (loginWindow != null)
            {
                Debug.Log("[PlayerLoginController] Mostrando ventana de login...");
                loginWindow.Show();
                presenter?.Reset();
            }
            else
            {
                Debug.LogError("[PlayerLoginController] ShowLogin llamado pero loginWindow es null");
            }
        }

        /// <summary>
        /// Oculta la ventana de login.
        /// </summary>
        public void HideLogin()
        {
            if (loginWindow != null)
            {
                loginWindow.Hide();
            }
        }

        private void OnPresenterLoginSuccessful(string playerName)
        {
            Debug.Log($"[PlayerLoginController] Login exitoso: {playerName}");
            
            // Guardar el nombre del jugador actual (para uso posterior)
            SaveCurrentPlayer(playerName);

            // Ocultar ventana y notificar
            HideLogin();
            OnLoginSuccess?.Invoke(playerName);
        }

        private void OnPresenterNewPlayerRequested(string playerName)
        {
            Debug.Log($"[PlayerLoginController] Nuevo jugador: {playerName}");

            // Registrar el jugador con contraseña por defecto
            RegisterNewPlayer(playerName);

            // Ocultar ventana y notificar
            HideLogin();
            OnNewPlayerCreation?.Invoke(playerName);
        }

        private void OnPresenterBackRequested()
        {
            Debug.Log("[PlayerLoginController] Back requested");
            HideLogin();
            OnBackToMenu?.Invoke();
        }

        private void SaveCurrentPlayer(string playerName)
        {
            // Guardar el nombre del jugador actual en ISaveService
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                saveService.SetString("current_player", playerName);
                saveService.Save();
            }
        }

        private void RegisterNewPlayer(string playerName)
        {
            // Registrar nuevo jugador con contraseña por defecto
            // El usuario podrá cambiar la contraseña después
            if (ServiceLocator.TryGet<IPlayerRepository>(out var repository))
            {
                repository.RegisterPlayer(playerName, PlayerLoginSettings.DefaultPassword);
                Debug.Log($"[PlayerLoginController] Jugador registrado con contraseña por defecto: {playerName}");
            }
        }
    }
}
