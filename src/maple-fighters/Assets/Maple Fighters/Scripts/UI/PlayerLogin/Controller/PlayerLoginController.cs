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
    /// Controlador principal del flujo de login de jugadores v2.
    /// Coordina la vista, el presenter y los servicios del juego.
    /// Soporta el flujo de email con selección de personaje integrada.
    /// Usa SessionManager para persistencia de sesión.
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

        [SerializeField]
        private bool enableAutoLogin = true;

        private PlayerLoginPresenter presenter;
        private IPlayerLoginApi cachedLoginApi;
        private ICredentialValidator cachedValidator;
        private ISessionManager sessionManager;
        private bool servicesInitialized;
        private bool windowCreatedDynamically;

        #region Events

        /// <summary>
        /// Se dispara cuando el login es exitoso y el jugador puede entrar al juego.
        /// Parámetros: (email, nombre del jugador, datos del jugador).
        /// </summary>
        public event Action<string, string, IPlayerCredentials> OnLoginSuccess;

        /// <summary>
        /// Se dispara cuando el usuario quiere volver (salir del login).
        /// </summary>
        public event Action OnBackToMenu;

        #endregion

        #region Properties

        /// <summary>
        /// Email del jugador actual (después de login exitoso).
        /// </summary>
        public string CurrentEmail { get; private set; }

        /// <summary>
        /// Nombre del jugador actual (después de login exitoso).
        /// </summary>
        public string CurrentPlayerName { get; private set; }

        /// <summary>
        /// Datos completos del jugador actual.
        /// </summary>
        public IPlayerCredentials CurrentPlayerData { get; private set; }

        /// <summary>
        /// Estado actual del presenter.
        /// </summary>
        public PlayerLoginState CurrentState => presenter?.CurrentState ?? PlayerLoginState.EnterEmail;

        /// <summary>
        /// Indica si hay una sesión activa.
        /// </summary>
        public bool HasSession => sessionManager?.HasValidSession ?? false;

        #endregion

        private void Start()
        {
            InitializeServices();
            
            if (loginWindow == null)
            {
                CreateLoginWindow();
            }

            InitializePresenter();

            // Intentar auto-login si está habilitado
            if (enableAutoLogin && TryAutoLogin())
            {
                return; // Auto-login exitoso, no mostrar UI
            }

            if (showOnStart && loginWindow != null)
            {
                ShowLogin();
            }
        }

        private void OnDestroy()
        {
            CleanupPresenter();
            
            if (windowCreatedDynamically && loginWindow != null)
            {
                Destroy(loginWindow.gameObject);
            }
        }

        private void CreateLoginWindow()
        {
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

            cachedLoginApi = ApiProvider.ProvidePlayerLoginApi();
            ServiceLocator.TryGet(out cachedValidator);
            ServiceLocator.TryGet(out sessionManager);

            if (cachedLoginApi == null)
            {
                Debug.LogWarning("[PlayerLoginController] No se pudo obtener IPlayerLoginApi");
            }

            if (cachedValidator == null)
            {
                Debug.LogWarning("[PlayerLoginController] No se pudo obtener ICredentialValidator");
            }

            if (sessionManager == null)
            {
                Debug.LogWarning("[PlayerLoginController] No se pudo obtener ISessionManager");
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
            presenter.BackRequested += OnPresenterBackRequested;
        }

        private void CleanupPresenter()
        {
            if (presenter != null)
            {
                presenter.LoginSuccessful -= OnPresenterLoginSuccessful;
                presenter.BackRequested -= OnPresenterBackRequested;
                presenter.Dispose();
                presenter = null;
            }
        }

        #region Auto-Login

        /// <summary>
        /// Intenta auto-login con sesión guardada usando SessionManager.
        /// </summary>
        /// <returns>True si auto-login exitoso.</returns>
        private bool TryAutoLogin()
        {
            if (sessionManager == null)
            {
                Debug.Log("[PlayerLoginController] SessionManager no disponible para auto-login");
                return false;
            }

            // Intentar cargar sesión guardada
            if (!sessionManager.LoadSession())
            {
                return false;
            }

            var session = sessionManager.CurrentSession;
            if (session == null || !session.IsValid())
            {
                return false;
            }

            // Obtener datos del jugador
            if (!ServiceLocator.TryGet<IPlayerRepository>(out var repository))
            {
                return false;
            }

            var player = repository.GetPlayerByEmail(session.Email);
            if (player == null || !player.IsRegistrationComplete())
            {
                Debug.Log("[PlayerLoginController] Jugador no encontrado o registro incompleto");
                sessionManager.ClearSession();
                return false;
            }

            // Auto-login exitoso
            Debug.Log($"[PlayerLoginController] Auto-login exitoso: {session.Email}");
            
            CurrentEmail = session.Email;
            CurrentPlayerName = session.PlayerName;
            CurrentPlayerData = player;
            
            // Actualizar última fecha de login y renovar sesión
            repository.UpdateLastLogin(session.Email);
            sessionManager.RefreshSession();
            
            OnLoginSuccess?.Invoke(session.Email, session.PlayerName, player);
            return true;
        }

        #endregion

        #region Public Methods

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

        /// <summary>
        /// Cierra la sesión actual y muestra el login.
        /// </summary>
        public void Logout()
        {
            sessionManager?.ClearSession();
            CurrentEmail = null;
            CurrentPlayerName = null;
            CurrentPlayerData = null;
            ShowLogin();
        }

        /// <summary>
        /// Fuerza mostrar el login sin intentar auto-login.
        /// </summary>
        public void ShowLoginForced()
        {
            sessionManager?.ClearSession();
            ShowLogin();
        }

        #endregion

        #region Event Handlers

        private void OnPresenterLoginSuccessful(string email, string playerName, IPlayerCredentials playerData)
        {
            Debug.Log($"[PlayerLoginController] Login exitoso: {email} -> {playerName}");
            
            CurrentEmail = email;
            CurrentPlayerName = playerName;
            CurrentPlayerData = playerData;

            // Crear sesión para auto-login futuro
            sessionManager?.CreateSession(email, playerName, playerData?.CharacterClass);

            // Guardar nombre del jugador actual para compatibilidad legacy
            SaveCurrentPlayerLegacy(playerName);

            HideLogin();
            OnLoginSuccess?.Invoke(email, playerName, playerData);
        }

        private void OnPresenterBackRequested()
        {
            Debug.Log("[PlayerLoginController] Back requested");
            HideLogin();
            OnBackToMenu?.Invoke();
        }

        private void SaveCurrentPlayerLegacy(string playerName)
        {
            // Compatibilidad con sistema legacy
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                saveService.SetString("current_player", playerName);
                saveService.Save();
            }
        }

        #endregion
    }
}
