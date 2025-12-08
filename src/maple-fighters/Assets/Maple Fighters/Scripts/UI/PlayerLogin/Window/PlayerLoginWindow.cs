using System;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Ventana de login de jugadores v2.
    /// Soporta flujo basado en email con selección de personaje integrada.
    /// </summary>
    [RequireComponent(typeof(UIFadeAnimation))]
    public class PlayerLoginWindow : UIElement, IPlayerLoginView
    {
        #region Events

        public event Action<string> ConfirmButtonClicked;
        public event Action BackButtonClicked;
        public event Action<string> InputFieldChanged;
        public event Action<string> CharacterClassSelected;
        public event Action<string, string> RegistrationConfirmed;

        #endregion

        #region Serialized Fields - Main Login Panel

        [Header("=== Main Login Panel ===")]
        [SerializeField]
        private GameObject loginPanel;

        [SerializeField]
        private InputField inputField;

        [SerializeField]
        private Button confirmButton;

        [SerializeField]
        private Button backButton;

        [SerializeField]
        private Text titleText;

        [SerializeField]
        private Text statusMessageText;

        [SerializeField]
        private Text placeholderText;

        [Header("=== Password Toggle ===")]
        [SerializeField]
        [Tooltip("Botón para mostrar/ocultar contraseña (icono de ojo)")]
        private Button togglePasswordButton;

        [SerializeField]
        [Tooltip("Icono cuando la contraseña está oculta (ojo cerrado). Opcional si usas texto.")]
        private GameObject hidePasswordIcon;

        [SerializeField]
        [Tooltip("Icono cuando la contraseña está visible (ojo abierto). Opcional si usas texto.")]
        private GameObject showPasswordIcon;

        [SerializeField]
        [Tooltip("Texto del botón (alternativa a iconos). Se actualiza automáticamente.")]
        private Text togglePasswordText;

        #endregion

        #region Serialized Fields - Character Selection Panel

        [Header("=== Character Selection Panel ===")]
        [SerializeField]
        private GameObject characterSelectionPanel;

        [SerializeField]
        private Button knightButton;

        [SerializeField]
        private Button archerButton;

        [SerializeField]
        private Button wizardButton;

        [SerializeField]
        private Image knightHighlight;

        [SerializeField]
        private Image archerHighlight;

        [SerializeField]
        private Image wizardHighlight;

        [SerializeField]
        private Button confirmCharacterButton;

        [SerializeField]
        private TextMeshProUGUI characterSelectionTitle;

        #endregion

        #region Serialized Fields - Registration Panel

        [Header("=== Registration Panel ===")]
        [SerializeField]
        private GameObject registrationPanel;

        [SerializeField]
        private TMP_InputField nameInputField;

        [SerializeField]
        private TMP_InputField passwordInputField;

        [SerializeField]
        private Button registerButton;

        [SerializeField]
        private TextMeshProUGUI registrationTitle;

        [SerializeField]
        private TextMeshProUGUI registrationStatusText;

        #endregion

        #region Private Fields

        private string currentEmail = string.Empty;
        private string selectedCharacterClass = string.Empty;
        private bool isPasswordVisible = false;
        private bool isInPasswordMode = false;

        #endregion

        #region Properties

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

        public string CurrentEmail
        {
            get => currentEmail;
            set => currentEmail = value;
        }

        public string SelectedCharacterClass
        {
            get => selectedCharacterClass;
            set => selectedCharacterClass = value;
        }

        #endregion

        #region Unity Lifecycle

        private void Awake()
        {
            var canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0;
                canvasGroup.blocksRaycasts = false;
                canvasGroup.interactable = false;
            }

            // Inicialmente ocultar paneles secundarios
            HideCharacterSelectionPanel();
            HideRegistrationPanel();
            
            // Ocultar botón de toggle de contraseña inicialmente
            if (togglePasswordButton != null)
            {
                togglePasswordButton.gameObject.SetActive(false);
            }
        }

        private void Start()
        {
            // Main panel events
            confirmButton?.onClick.AddListener(OnConfirmButtonClicked);
            backButton?.onClick.AddListener(OnBackButtonClicked);
            inputField?.onValueChanged.AddListener(OnInputFieldChanged);
            togglePasswordButton?.onClick.AddListener(OnTogglePasswordVisibility);

            // Character selection events
            knightButton?.onClick.AddListener(() => OnCharacterSelected("Knight"));
            archerButton?.onClick.AddListener(() => OnCharacterSelected("Archer"));
            wizardButton?.onClick.AddListener(() => OnCharacterSelected("Wizard"));
            confirmCharacterButton?.onClick.AddListener(OnConfirmCharacterClicked);

            // Registration panel events
            registerButton?.onClick.AddListener(OnRegisterButtonClicked);
        }

        private void OnDestroy()
        {
            // Cleanup main panel
            confirmButton?.onClick.RemoveAllListeners();
            backButton?.onClick.RemoveAllListeners();
            inputField?.onValueChanged.RemoveAllListeners();
            togglePasswordButton?.onClick.RemoveAllListeners();

            // Cleanup character selection
            knightButton?.onClick.RemoveAllListeners();
            archerButton?.onClick.RemoveAllListeners();
            wizardButton?.onClick.RemoveAllListeners();
            confirmCharacterButton?.onClick.RemoveAllListeners();

            // Cleanup registration
            registerButton?.onClick.RemoveAllListeners();
        }

        #endregion

        #region Button Control

        public void EnableConfirmButton()
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = true;
            }
        }

        public void DisableConfirmButton()
        {
            if (confirmButton != null)
            {
                confirmButton.interactable = false;
            }
        }

        public void EnableInteraction()
        {
            if (inputField != null) inputField.interactable = true;
            EnableConfirmButton();
            if (backButton != null) backButton.interactable = true;
        }

        public void DisableInteraction()
        {
            if (inputField != null) inputField.interactable = false;
            DisableConfirmButton();
            if (backButton != null) backButton.interactable = false;
        }

        #endregion

        #region Input Configuration

        public void SetPasswordMode(bool isPassword)
        {
            isInPasswordMode = isPassword;
            isPasswordVisible = false; // Reset visibility when changing mode
            
            if (inputField != null)
            {
                inputField.contentType = isPassword
                    ? InputField.ContentType.Password
                    : InputField.ContentType.Standard;
                inputField.ForceLabelUpdate();
            }
            
            // Mostrar/ocultar botón de toggle según el modo
            UpdatePasswordToggleButton();
        }

        /// <summary>
        /// Toggle de visibilidad de la contraseña (llamado por el botón de ojo)
        /// </summary>
        private void OnTogglePasswordVisibility()
        {
            if (!isInPasswordMode) return;
            
            isPasswordVisible = !isPasswordVisible;
            
            if (inputField != null)
            {
                // Guardar texto actual y posición del cursor
                string currentText = inputField.text;
                int caretPosition = inputField.caretPosition;
                
                inputField.contentType = isPasswordVisible
                    ? InputField.ContentType.Standard
                    : InputField.ContentType.Password;
                
                // Restaurar texto y forzar actualización
                inputField.text = currentText;
                inputField.ForceLabelUpdate();
                
                // Restaurar foco y posición del cursor
                inputField.Select();
                inputField.caretPosition = caretPosition;
            }
            
            UpdatePasswordToggleIcons();
        }

        /// <summary>
        /// Actualiza la visibilidad del botón de toggle de contraseña
        /// </summary>
        private void UpdatePasswordToggleButton()
        {
            if (togglePasswordButton != null)
            {
                togglePasswordButton.gameObject.SetActive(isInPasswordMode);
            }
            
            UpdatePasswordToggleIcons();
        }

        /// <summary>
        /// Actualiza los iconos del botón según el estado de visibilidad
        /// </summary>
        private void UpdatePasswordToggleIcons()
        {
            // Si la contraseña está visible, mostrar icono de "ocultar" (ojo abierto)
            // Si la contraseña está oculta, mostrar icono de "mostrar" (ojo cerrado)
            if (hidePasswordIcon != null)
            {
                hidePasswordIcon.SetActive(!isPasswordVisible);
            }
            
            if (showPasswordIcon != null)
            {
                showPasswordIcon.SetActive(isPasswordVisible);
            }
            
            // Alternativa: actualizar texto del botón si no hay iconos
            if (togglePasswordText != null)
            {
                // Usar símbolos Unicode como alternativa a iconos
                // 👁 (ojo) cuando está oculto = "mostrar"
                // ✕ o 👁‍🗨 cuando está visible = "ocultar"
                togglePasswordText.text = isPasswordVisible ? "⊗" : "👁";
            }
        }

        public void ClearInput()
        {
            if (inputField != null)
            {
                inputField.text = string.Empty;
            }
        }

        public void FocusInput()
        {
            if (inputField != null)
            {
                inputField.Select();
                inputField.ActivateInputField();
            }
        }

        #endregion

        #region Character Selection Panel

        public void ShowCharacterSelectionPanel()
        {
            if (characterSelectionPanel != null)
            {
                characterSelectionPanel.SetActive(true);
            }
            
            // Reset selection
            selectedCharacterClass = string.Empty;
            ClearCharacterHighlights();

            if (confirmCharacterButton != null)
            {
                confirmCharacterButton.interactable = false;
            }

            if (characterSelectionTitle != null)
            {
                characterSelectionTitle.text = $"Selecciona tu personaje\n({currentEmail})";
            }
        }

        public void HideCharacterSelectionPanel()
        {
            if (characterSelectionPanel != null)
            {
                characterSelectionPanel.SetActive(false);
            }
        }

        public void HighlightSelectedCharacter(string characterClass)
        {
            ClearCharacterHighlights();

            switch (characterClass)
            {
                case "Knight":
                    if (knightHighlight != null) knightHighlight.enabled = true;
                    break;
                case "Archer":
                    if (archerHighlight != null) archerHighlight.enabled = true;
                    break;
                case "Wizard":
                    if (wizardHighlight != null) wizardHighlight.enabled = true;
                    break;
            }
        }

        private void ClearCharacterHighlights()
        {
            if (knightHighlight != null) knightHighlight.enabled = false;
            if (archerHighlight != null) archerHighlight.enabled = false;
            if (wizardHighlight != null) wizardHighlight.enabled = false;
        }

        #endregion

        #region Registration Panel

        public void ShowRegistrationPanel()
        {
            if (registrationPanel != null)
            {
                registrationPanel.SetActive(true);
            }

            ClearRegistrationFields();

            if (registrationTitle != null)
            {
                registrationTitle.text = $"Completa tu registro\n({selectedCharacterClass})";
            }

            if (registrationStatusText != null)
            {
                registrationStatusText.text = string.Empty;
            }
        }

        public void HideRegistrationPanel()
        {
            if (registrationPanel != null)
            {
                registrationPanel.SetActive(false);
            }
        }

        public string GetRegistrationName()
        {
            return nameInputField != null ? nameInputField.text : string.Empty;
        }

        public string GetRegistrationPassword()
        {
            return passwordInputField != null ? passwordInputField.text : string.Empty;
        }

        public void ClearRegistrationFields()
        {
            if (nameInputField != null) nameInputField.text = string.Empty;
            if (passwordInputField != null) passwordInputField.text = string.Empty;
        }

        #endregion

        #region Panel Visibility

        public void ShowLoginPanel()
        {
            if (loginPanel != null)
            {
                loginPanel.SetActive(true);
            }
        }

        public void HideLoginPanel()
        {
            if (loginPanel != null)
            {
                loginPanel.SetActive(false);
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

        private void OnCharacterSelected(string characterClass)
        {
            selectedCharacterClass = characterClass;
            HighlightSelectedCharacter(characterClass);
            
            if (confirmCharacterButton != null)
            {
                confirmCharacterButton.interactable = true;
            }

            CharacterClassSelected?.Invoke(characterClass);
        }

        private void OnConfirmCharacterClicked()
        {
            if (!string.IsNullOrEmpty(selectedCharacterClass))
            {
                // Trigger transition to registration panel
                HideCharacterSelectionPanel();
                ShowRegistrationPanel();
            }
        }

        private void OnRegisterButtonClicked()
        {
            var name = GetRegistrationName();
            var password = GetRegistrationPassword();
            
            RegistrationConfirmed?.Invoke(name, password);
        }

        #endregion
    }
}
