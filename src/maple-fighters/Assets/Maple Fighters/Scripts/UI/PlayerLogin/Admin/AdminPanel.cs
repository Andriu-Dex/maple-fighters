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

            // Verificar que adminService esté disponible
            if (adminService == null)
            {
                InitializeServices();
                if (adminService == null)
                {
                    isVisible = false;
                    return;
                }
            }

            // Escalar UI basado en resolución (referencia: 1920x1080)
            float scaleFactor = Screen.height / 1080f;
            scaleFactor = Mathf.Clamp(scaleFactor, 1.0f, 2.5f);

            // Definir área del panel - escalado
            var panelWidth = 600f * scaleFactor;
            var panelHeight = 650f * scaleFactor;
            var panelX = (Screen.width - panelWidth) / 2;
            var panelY = (Screen.height - panelHeight) / 2;

            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Fondo semi-transparente
            GUI.Box(panelRect, "");

            GUILayout.BeginArea(panelRect);
            GUILayout.BeginVertical(GUI.skin.box);

            // Título
            GUILayout.Label("Panel de Administrador", GetTitleStyle(scaleFactor));
            GUILayout.Space(20 * scaleFactor);

            // Información - Mostrar email si no hay nombre
            var displayName = !string.IsNullOrEmpty(currentPlayerName) ? currentPlayerName : currentEmail;
            GUILayout.Label($"Usuario: {displayName}", GetInfoStyle(scaleFactor));
            
            var blockedCount = adminService.GetBlockedPlayersCount();
            GUILayout.Label($"Jugadores bloqueados: {blockedCount}", GetInfoStyle(scaleFactor));

            GUILayout.Space(20 * scaleFactor);

            // Botón para desbloquear todos
            if (blockedCount > 0)
            {
                if (GUILayout.Button("Desbloquear Todos", GetButtonStyle(scaleFactor), GUILayout.Height(50 * scaleFactor)))
                {
                    // Usar email como identificador de admin si no hay nombre
                    var adminId = !string.IsNullOrEmpty(currentPlayerName) ? currentPlayerName : "Admin";
                    if (adminService.UnblockAllPlayers(adminId))
                    {
                        Debug.Log("[AdminPanel] Todos los jugadores desbloqueados");
                    }
                }

                GUILayout.Space(15 * scaleFactor);

                // Lista de jugadores bloqueados
                GUILayout.Label("Lista de jugadores bloqueados:", GetSubtitleStyle(scaleFactor));

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(250 * scaleFactor));

                var blockedPlayers = adminService.GetBlockedPlayers("Admin");
                foreach (var player in blockedPlayers)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(player, GetInfoStyle(scaleFactor), GUILayout.Width(280 * scaleFactor));
                    
                    if (GUILayout.Button("Desbloquear", GetButtonStyle(scaleFactor), GUILayout.Width(140 * scaleFactor), GUILayout.Height(35 * scaleFactor)))
                    {
                        adminService.UnblockPlayer("Admin", player);
                    }
                    
                    GUILayout.EndHorizontal();
                    GUILayout.Space(8 * scaleFactor);
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Space(25 * scaleFactor);
                GUILayout.Label("No hay jugadores bloqueados.", GetInfoStyle(scaleFactor));
                GUILayout.FlexibleSpace();
            }

            GUILayout.Space(15 * scaleFactor);

            // Botón cerrar
            if (GUILayout.Button("Cerrar (F12)", GetButtonStyle(scaleFactor), GUILayout.Height(45 * scaleFactor)))
            {
                Hide();
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private GUIStyle GetTitleStyle(float scale)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(28 * scale),
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            return style;
        }

        private GUIStyle GetSubtitleStyle(float scale)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(20 * scale),
                fontStyle = FontStyle.Bold
            };
            return style;
        }

        private GUIStyle GetInfoStyle(float scale)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(18 * scale)
            };
            return style;
        }

        private GUIStyle GetButtonStyle(float scale)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(18 * scale)
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
