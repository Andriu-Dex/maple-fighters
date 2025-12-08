using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Panel de administrador usando OnGUI para simplicidad.
    /// Permite al usuario Admin desbloquear jugadores bloqueados.
    /// Implementa patrón Singleton para acceso global.
    /// </summary>
    public class AdminPanel : MonoBehaviour
    {
        #region Singleton

        private static AdminPanel instance;
        
        /// <summary>
        /// Instancia singleton del AdminPanel.
        /// </summary>
        public static AdminPanel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<AdminPanel>();
                }
                return instance;
            }
        }

        #endregion

        #region Events

        /// <summary>
        /// Se dispara cuando el panel se cierra.
        /// </summary>
        public event Action OnPanelClosed;

        #endregion

        [Header("Settings")]
        [SerializeField]
        private KeyCode toggleKey = KeyCode.F12;

        [SerializeField]
        private bool showInBuild = true;

        private IAdminService adminService;
        private string currentPlayerName;
        private string currentEmail;
        private bool isVisible;
        private Vector2 scrollPosition;
        private bool isInitialized;

        private void Awake()
        {
            // Singleton setup
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;
            
            // Persistir entre escenas para poder usar F12 en cualquier momento
            DontDestroyOnLoad(gameObject);
            
            InitializeServices();
        }

        private void InitializeServices()
        {
            // Siempre intentar obtener el servicio si es null
            // (puede ser null después de cambio de escena aunque isInitialized sea true)
            if (adminService == null)
            {
                ServiceLocator.TryGet(out adminService);
                Debug.Log($"[AdminPanel] InitializeServices: adminService={(adminService != null ? "OK" : "NULL")}");
            }

            if (isInitialized) return;

            // Obtener nombre del jugador actual
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                currentPlayerName = saveService.GetString("current_player", string.Empty);
            }
            
            isInitialized = true;
        }

        private void Update()
        {
            // Toggle del panel con tecla (solo si ya hay un usuario admin logueado)
            if (Input.GetKeyDown(toggleKey))
            {
                Debug.Log($"[AdminPanel] F12 presionado. showInBuild={showInBuild}, isEditor={Application.isEditor}");
                
                // Solo permitir en editor o si showInBuild está activo
                if (Application.isEditor || showInBuild)
                {
                    Debug.Log($"[AdminPanel] Llamando TogglePanel...");
                    TogglePanel();
                }
            }
        }

        /// <summary>
        /// Alterna la visibilidad del panel.
        /// </summary>
        public void TogglePanel()
        {
            Debug.Log($"[AdminPanel] TogglePanel iniciado. currentPlayerName='{currentPlayerName}', currentEmail='{currentEmail}'");
            
            InitializeServices();
            
            bool isAdmin = IsCurrentPlayerAdmin();
            Debug.Log($"[AdminPanel] IsCurrentPlayerAdmin()={isAdmin}");
            
            // Verificar si el usuario actual es admin (por nombre o por email)
            if (!isAdmin)
            {
                Debug.LogWarning($"[AdminPanel] Solo el administrador puede acceder a este panel. Usuario actual: '{currentPlayerName}', Email: '{currentEmail}'");
                return;
            }

            isVisible = !isVisible;
            Debug.Log($"[AdminPanel] Panel visibilidad cambiada a: {isVisible}");
            
            if (!isVisible)
            {
                OnPanelClosed?.Invoke();
            }
        }

        /// <summary>
        /// Muestra el panel de administración.
        /// </summary>
        public void Show()
        {
            InitializeServices();
            
            // Verificar si el usuario actual es admin (por nombre o por email)
            if (!IsCurrentPlayerAdmin())
            {
                Debug.LogWarning($"[AdminPanel] Solo el administrador puede acceder a este panel. Usuario actual: '{currentPlayerName}', Email: '{currentEmail}'");
                return;
            }

            isVisible = true;
            Debug.Log($"[AdminPanel] Panel mostrado para: {currentPlayerName} ({currentEmail})");
        }

        /// <summary>
        /// Oculta el panel de administración.
        /// </summary>
        public void Hide()
        {
            if (isVisible)
            {
                isVisible = false;
                OnPanelClosed?.Invoke();
            }
        }

        /// <summary>
        /// Indica si el panel está visible.
        /// </summary>
        public bool IsVisible => isVisible;

        /// <summary>
        /// Establece el nombre del jugador actual.
        /// </summary>
        public void SetCurrentPlayer(string playerName)
        {
            currentPlayerName = playerName;
            Debug.Log($"[AdminPanel] Usuario actual establecido: {playerName}");
        }

        /// <summary>
        /// Establece el nombre y email del jugador actual.
        /// </summary>
        public void SetCurrentPlayer(string playerName, string email)
        {
            currentPlayerName = playerName;
            currentEmail = email;
            Debug.Log($"[AdminPanel] Usuario actual establecido: {playerName} ({email})");
        }

        /// <summary>
        /// Verifica si el jugador actual es administrador.
        /// </summary>
        public bool IsCurrentPlayerAdmin()
        {
            InitializeServices();
            
            if (adminService == null)
            {
                return false;
            }
            
            // Verificar por nombre O por email (sin logs para evitar spam en OnGUI)
            return adminService.IsAdmin(currentPlayerName) || adminService.IsAdminByEmail(currentEmail);
        }

        /// <summary>
        /// Verifica si el jugador actual es administrador (con logs de debug).
        /// </summary>
        private bool IsCurrentPlayerAdminWithLogs()
        {
            InitializeServices();
            
            if (adminService == null)
            {
                Debug.LogWarning("[AdminPanel] adminService es NULL!");
                return false;
            }
            
            bool isAdminByName = adminService.IsAdmin(currentPlayerName);
            bool isAdminByEmail = adminService.IsAdminByEmail(currentEmail);
            
            Debug.Log($"[AdminPanel] IsAdmin('{currentPlayerName}')={isAdminByName}, IsAdminByEmail('{currentEmail}')={isAdminByEmail}");
            
            return isAdminByName || isAdminByEmail;
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            // Verificar permisos (por nombre o email) - sin logs
            if (!IsCurrentPlayerAdmin())
            {
                isVisible = false;
                return;
            }

            // Definir área del panel
            var panelWidth = 450f;
            var panelHeight = 500f;
            var panelX = (Screen.width - panelWidth) / 2;
            var panelY = (Screen.height - panelHeight) / 2;

            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Fondo semi-transparente
            GUI.Box(panelRect, "");

            GUILayout.BeginArea(panelRect);
            GUILayout.BeginVertical(GUI.skin.box);

            // Título
            GUILayout.Label("Panel de Administrador", GetTitleStyle());
            GUILayout.Space(15);

            // Información - Mostrar email si no hay nombre
            var displayName = !string.IsNullOrEmpty(currentPlayerName) ? currentPlayerName : currentEmail;
            GUILayout.Label($"Usuario: {displayName}", GetInfoStyle());
            
            var blockedCount = adminService.GetBlockedPlayersCount();
            GUILayout.Label($"Jugadores bloqueados: {blockedCount}", GetInfoStyle());

            GUILayout.Space(15);

            // Botón para desbloquear todos
            if (blockedCount > 0)
            {
                if (GUILayout.Button("Desbloquear Todos", GUILayout.Height(45)))
                {
                    // Usar email como identificador de admin si no hay nombre
                    var adminId = !string.IsNullOrEmpty(currentPlayerName) ? currentPlayerName : "Admin";
                    if (adminService.UnblockAllPlayers(adminId))
                    {
                        Debug.Log("[AdminPanel] Todos los jugadores desbloqueados");
                    }
                }

                GUILayout.Space(15);

                // Lista de jugadores bloqueados
                GUILayout.Label("Lista de jugadores bloqueados:", GetSubtitleStyle());

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250));

                var blockedPlayers = adminService.GetBlockedPlayers("Admin");
                foreach (var player in blockedPlayers)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(player, GUILayout.Width(250));
                    
                    if (GUILayout.Button("Desbloquear", GUILayout.Width(120), GUILayout.Height(30)))
                    {
                        adminService.UnblockPlayer("Admin", player);
                    }
                    
                    GUILayout.EndHorizontal();
                    GUILayout.Space(5);
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Space(20);
                GUILayout.Label("No hay jugadores bloqueados.", GetInfoStyle());
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(10);

            // Botón cerrar
            if (GUILayout.Button("Cerrar (F12)", GUILayout.Height(40)))
            {
                Hide();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private GUIStyle GetTitleStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 22,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            return style;
        }

        private GUIStyle GetSubtitleStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14,
                fontStyle = FontStyle.Bold
            };
            return style;
        }

        private GUIStyle GetInfoStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 14
            };
            return style;
        }

        private void OnDestroy()
        {
            if (instance == this)
            {
                instance = null;
            }
        }
    }
}
