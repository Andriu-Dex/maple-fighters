using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Panel de administrador usando OnGUI para simplicidad.
    /// Permite al usuario Admin desbloquear jugadores bloqueados.
    /// </summary>
    public class AdminPanel : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField]
        private KeyCode toggleKey = KeyCode.F12;

        [SerializeField]
        private bool showInBuild = false;

        private IAdminService adminService;
        private string currentPlayerName;
        private bool isVisible;
        private Vector2 scrollPosition;

        private void Awake()
        {
            // Obtener servicio de admin del ServiceLocator
            ServiceLocator.TryGet(out adminService);

            // Obtener nombre del jugador actual
            if (ServiceLocator.TryGet<ISaveService>(out var saveService))
            {
                currentPlayerName = saveService.GetString("current_player", string.Empty);
            }
        }

        private void Update()
        {
            // Toggle del panel con tecla
            if (Input.GetKeyDown(toggleKey))
            {
                // Solo permitir en editor o si showInBuild está activo
                if (Application.isEditor || showInBuild)
                {
                    TogglePanel();
                }
            }
        }

        /// <summary>
        /// Alterna la visibilidad del panel.
        /// </summary>
        public void TogglePanel()
        {
            // Verificar si el usuario actual es admin
            if (adminService == null || !adminService.IsAdmin(currentPlayerName))
            {
                Debug.LogWarning("[AdminPanel] Solo el administrador puede acceder a este panel");
                return;
            }

            isVisible = !isVisible;
        }

        /// <summary>
        /// Establece el nombre del jugador actual.
        /// </summary>
        public void SetCurrentPlayer(string playerName)
        {
            currentPlayerName = playerName;
        }

        private void OnGUI()
        {
            if (!isVisible) return;

            // Verificar permisos
            if (adminService == null || !adminService.IsAdmin(currentPlayerName))
            {
                isVisible = false;
                return;
            }

            // Definir área del panel
            var panelWidth = 350f;
            var panelHeight = 400f;
            var panelX = (Screen.width - panelWidth) / 2;
            var panelY = (Screen.height - panelHeight) / 2;

            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Fondo semi-transparente
            GUI.Box(panelRect, "");

            GUILayout.BeginArea(panelRect);
            GUILayout.BeginVertical(GUI.skin.box);

            // Título
            GUILayout.Label("Panel de Administrador", GetTitleStyle());
            GUILayout.Space(10);

            // Información
            GUILayout.Label($"Usuario: {currentPlayerName}");
            
            var blockedCount = adminService.GetBlockedPlayersCount();
            GUILayout.Label($"Jugadores bloqueados: {blockedCount}");

            GUILayout.Space(10);

            // Botón para desbloquear todos
            if (blockedCount > 0)
            {
                if (GUILayout.Button("Desbloquear Todos", GUILayout.Height(40)))
                {
                    if (adminService.UnblockAllPlayers(currentPlayerName))
                    {
                        Debug.Log("[AdminPanel] Todos los jugadores desbloqueados");
                    }
                }

                GUILayout.Space(10);

                // Lista de jugadores bloqueados
                GUILayout.Label("Jugadores bloqueados:");

                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(200));

                var blockedPlayers = adminService.GetBlockedPlayers(currentPlayerName);
                foreach (var player in blockedPlayers)
                {
                    GUILayout.BeginHorizontal();
                    GUILayout.Label(player, GUILayout.Width(200));
                    
                    if (GUILayout.Button("Desbloquear", GUILayout.Width(100)))
                    {
                        adminService.UnblockPlayer(currentPlayerName, player);
                    }
                    
                    GUILayout.EndHorizontal();
                }

                GUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No hay jugadores bloqueados.");
            }

            GUILayout.FlexibleSpace();

            // Botón cerrar
            if (GUILayout.Button("Cerrar (F12)", GUILayout.Height(30)))
            {
                isVisible = false;
            }

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private GUIStyle GetTitleStyle()
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = 18,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.MiddleCenter
            };
            return style;
        }
    }
}
