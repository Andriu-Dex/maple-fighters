using System;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Ventana de login de jugadores.
    /// Reutiliza el diseño visual de CharacterNameWindow con lógica propia.
    /// 
    /// Para usar: duplicar el prefab de CharacterNameWindow en Unity,
    /// reemplazar el script por este, y agregar el Text para título y mensaje.
    /// </summary>
    [RequireComponent(typeof(UIFadeAnimation))]
    public class PlayerLoginWindow : UIElement, IPlayerLoginView
    {
        #region Events
        /// <inheritdoc/>
        public event Action<string> ConfirmButtonClicked;

        /// <inheritdoc/>
        public event Action BackButtonClicked;

        /// <inheritdoc/>
        public event Action<string> InputFieldChanged;
        #endregion

        #region Serialized Fields
        [Header("Input")]
        [SerializeField]
        private InputField inputField;

        [Header("Buttons")]
        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button backButton;

        [Header("Text Elements")]
        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Text statusMessageText;

        [SerializeField]
        private Text placeholderText;
        #endregion

        #region Properties
        /// <inheritdoc/>
        public string InputText
        {
            get => inputField != null ? inputField.text : string.Empty;
            set
            {
                if (inputField != null)
                {
                    inputField.text = value;
                }
            }
        }

        /// <inheritdoc/>
        public string Title
        {
            set
            {
                if (titleText != null)
                {
                    titleText.text = value;
                }
            }
        }

        /// <inheritdoc/>
        public string Placeholder
        {
            set
            {
                if (placeholderText != null)
                {
                    placeholderText.text = value;
                }
            }
        }

        /// <inheritdoc/>
        public string StatusMessage
        {
            set
            {
                if (statusMessageText != null)
                {
                    statusMessageText.text = value;
                    statusMessageText.gameObject.SetActive(!string.IsNullOrEmpty(value));
                }
            }
        }
        #endregion

        #region Unity Lifecycle
        private void Awake()
        {
            // Inicialmente oculto (alpha = 0)
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }
        }

        private void Start()
        {
            confirmButton?.onClick.AddListener(OnConfirmButtonClicked);
            backButton?.onClick.AddListener(OnBackButtonClicked);
            inputField?.onValueChanged.AddListener(OnInputFieldChanged);
        }

        private void OnDestroy()
        {
            confirmButton?.onClick.RemoveListener(OnConfirmButtonClicked);
            backButton?.onClick.RemoveListener(OnBackButtonClicked);
            inputField?.onValueChanged.RemoveListener(OnInputFieldChanged);
        }
        #endregion

        #region IPlayerLoginView Implementation
        /// <inheritdoc/>
        public void EnableConfirmButton()
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }

        /// <inheritdoc/>
        public void DisableConfirmButton()
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }
        }

        /// <inheritdoc/>
        public void EnableInteraction()
        {
            if (inputField != null)
            {
                inputField.interactable = true;
            }
            EnableConfirmButton();
            if (backButton != null)
            {
                backButton.interactable = true;
            }
        }

        /// <inheritdoc/>
        public void DisableInteraction()
        {
            if (inputField != null)
            {
                inputField.interactable = false;
            }
            DisableConfirmButton();
            if (backButton != null)
            {
                backButton.interactable = false;
            }
        }

        /// <inheritdoc/>
        public void SetPasswordMode(bool isPassword)
        {
            if (inputField != null)
            {
                inputField.contentType = isPassword 
                    ? InputField.ContentType.Password 
                    : InputField.ContentType.Standard;
                
                // Forzar actualización del input field
                inputField.ForceLabelUpdate();
            }
        }

        /// <inheritdoc/>
        public void ClearInput()
        {
            if (inputField != null)
            {
                inputField.text = string.Empty;
            }
        }

        /// <inheritdoc/>
        public void FocusInput()
        {
            if (inputField != null)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
        }
        #endregion

        #region Event Handlers
        private void OnConfirmButtonClicked()
        {
            var text = inputField != null ? inputField.text : string.Empty;
            ConfirmButtonClicked?.Invoke(text);
        }

        private void OnBackButtonClicked()
        {
            BackButtonClicked?.Invoke();
        }

        private void OnInputFieldChanged(string text)
        {
            InputFieldChanged?.Invoke(text);
        }
        #endregion
    }
}
