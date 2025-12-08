using Scripts.Constants;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Services;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.UI.GameMenu
{
    /// <summary>
    /// Panel de menú del juego que aparece con la tecla Escape.
    /// Permite al usuario cerrar sesión y volver a la pantalla de login.
    /// </summary>
    public class GameMenuPanel : MonoBehaviour
    {
        #region Singleton

        private static GameMenuPanel instance;

        public static GameMenuPanel Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindObjectOfType<GameMenuPanel>();
                }
                return instance;
            }
        }

        #endregion

        [Header("Settings")]
        [SerializeField]
        private KeyCode toggleKey = KeyCode.Escape;

        private bool isVisible;
        private ISaveService saveService;
        private ISessionManager sessionManager;

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }
            instance = this;

            DontDestroyOnLoad(gameObject);

            ServiceLocator.TryGet(out saveService);
            ServiceLocator.TryGet(out sessionManager);
        }

        private void Update()
        {
            if (Input.GetKeyDown(toggleKey))
            {
                TogglePanel();
            }
        }

        public void TogglePanel()
        {
            isVisible = !isVisible;
        }

        public void Show()
        {
            isVisible = true;
        }

        public void Hide()
        {
            isVisible = false;
        }

        public bool IsVisible => isVisible;

        private void OnGUI()
        {
            if (!isVisible) return;

            // Escalar UI basado en resolución
            float scaleFactor = Screen.height / 1080f;
            scaleFactor = Mathf.Clamp(scaleFactor, 1.0f, 2.5f);

            // Definir área del panel
            var panelWidth = 400f * scaleFactor;
            var panelHeight = 300f * scaleFactor;
            var panelX = (Screen.width - panelWidth) / 2;
            var panelY = (Screen.height - panelHeight) / 2;

            var panelRect = new Rect(panelX, panelY, panelWidth, panelHeight);

            // Fondo
            GUI.Box(panelRect, "");

            GUILayout.BeginArea(panelRect);
            GUILayout.BeginVertical(GUI.skin.box);

            // Título
            GUILayout.Label("Menú", GetTitleStyle(scaleFactor));
            GUILayout.Space(30 * scaleFactor);

            // Botón Continuar
            if (GUILayout.Button("Continuar", GetButtonStyle(scaleFactor), GUILayout.Height(50 * scaleFactor)))
            {
                Hide();
            }

            GUILayout.Space(15 * scaleFactor);

            // Botón Cerrar Sesión
            if (GUILayout.Button("Cerrar Sesión", GetButtonStyle(scaleFactor), GUILayout.Height(50 * scaleFactor)))
            {
                Logout();
            }

            GUILayout.FlexibleSpace();

            // Instrucción
            GUILayout.Label("Presiona ESC para cerrar", GetInfoStyle(scaleFactor));

            GUILayout.EndVertical();
            GUILayout.EndArea();
        }

        private void Logout()
        {
            Hide();

            // Limpiar sesión
            sessionManager?.ClearSession();

            // Limpiar UserMetadata
            var userMetadata = FindObjectOfType<UserMetadata>();
            if (userMetadata != null)
            {
                userMetadata.ClearSession();
            }

            Debug.Log("[GameMenuPanel] Sesión cerrada, volviendo al login...");

            // Volver a la escena principal (login)
            SceneManager.LoadScene(SceneNames.Main);
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

        private GUIStyle GetButtonStyle(float scale)
        {
            var style = new GUIStyle(GUI.skin.button)
            {
                fontSize = Mathf.RoundToInt(20 * scale)
            };
            return style;
        }

        private GUIStyle GetInfoStyle(float scale)
        {
            var style = new GUIStyle(GUI.skin.label)
            {
                fontSize = Mathf.RoundToInt(14 * scale),
                alignment = TextAnchor.MiddleCenter
            };
            style.normal.textColor = Color.gray;
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
