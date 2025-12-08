using System;
using UI;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Interface para la vista de login de jugadores.
    /// Sigue el patrón MVP (Model-View-Presenter).
    /// Soporta flujo v2 (email-based) con selección de personaje.
    /// </summary>
    public interface IPlayerLoginView : IView
    {
        #region Events

        /// <summary>
        /// Se dispara cuando el usuario confirma la entrada principal (email/contraseña).
        /// Parámetro: texto ingresado.
        /// </summary>
        event Action<string> ConfirmButtonClicked;

        /// <summary>
        /// Se dispara cuando el usuario presiona el botón de volver.
        /// </summary>
        event Action BackButtonClicked;

        /// <summary>
        /// Se dispara cuando cambia el texto del input principal.
        /// </summary>
        event Action<string> InputFieldChanged;

        /// <summary>
        /// Se dispara cuando el usuario selecciona una clase de personaje.
        /// Parámetro: nombre de la clase (e.g., "Knight", "Archer", "Wizard").
        /// </summary>
        event Action<string> CharacterClassSelected;

        /// <summary>
        /// Se dispara cuando el usuario confirma el registro completo.
        /// Parámetros: (nombre, contraseña).
        /// </summary>
        event Action<string, string> RegistrationConfirmed;

        #endregion

        #region Properties

        /// <summary>
        /// Texto actual del campo de entrada principal.
        /// </summary>
        string InputText { get; set; }

        /// <summary>
        /// Título de la ventana.
        /// </summary>
        string Title { set; }

        /// <summary>
        /// Texto del placeholder del input principal.
        /// </summary>
        string Placeholder { set; }

        /// <summary>
        /// Mensaje de estado/error para mostrar al usuario.
        /// </summary>
        string StatusMessage { set; }

        /// <summary>
        /// Email actualmente ingresado (para mostrar en pasos posteriores).
        /// </summary>
        string CurrentEmail { get; set; }

        /// <summary>
        /// Clase de personaje seleccionada.
        /// </summary>
        string SelectedCharacterClass { get; set; }

        #endregion

        #region Button Control

        /// <summary>
        /// Habilita el botón de confirmar.
        /// </summary>
        void EnableConfirmButton();

        /// <summary>
        /// Deshabilita el botón de confirmar.
        /// </summary>
        void DisableConfirmButton();

        /// <summary>
        /// Habilita el botón de retroceso (Back).
        /// Útil para estados como Blocked donde se debe permitir volver.
        /// </summary>
        void EnableBackButton();

        /// <summary>
        /// Deshabilita/oculta el botón de retroceso (Back).
        /// Útil para estados iniciales donde no hay adonde volver.
        /// </summary>
        void DisableBackButton();

        /// <summary>
        /// Habilita toda la interacción con la vista.
        /// </summary>
        void EnableInteraction();

        /// <summary>
        /// Deshabilita toda la interacción con la vista.
        /// </summary>
        void DisableInteraction();

        #endregion

        #region Input Configuration

        /// <summary>
        /// Configura el input como campo de contraseña (oculta caracteres).
        /// </summary>
        void SetPasswordMode(bool isPassword);

        /// <summary>
        /// Limpia el campo de entrada principal.
        /// </summary>
        void ClearInput();

        /// <summary>
        /// Enfoca el campo de entrada principal.
        /// </summary>
        void FocusInput();

        #endregion

        #region v2 - Character Selection Panel

        /// <summary>
        /// Muestra el panel de selección de personaje.
        /// </summary>
        void ShowCharacterSelectionPanel();

        /// <summary>
        /// Oculta el panel de selección de personaje.
        /// </summary>
        void HideCharacterSelectionPanel();

        /// <summary>
        /// Resalta el personaje seleccionado visualmente.
        /// </summary>
        /// <param name="characterClass">Clase del personaje a resaltar.</param>
        void HighlightSelectedCharacter(string characterClass);

        #endregion

        #region v2 - Registration Panel

        /// <summary>
        /// Muestra el panel de registro (nombre + contraseña).
        /// </summary>
        void ShowRegistrationPanel();

        /// <summary>
        /// Oculta el panel de registro.
        /// </summary>
        void HideRegistrationPanel();

        /// <summary>
        /// Obtiene el nombre ingresado en el panel de registro.
        /// </summary>
        string GetRegistrationName();

        /// <summary>
        /// Obtiene la contraseña ingresada en el panel de registro.
        /// </summary>
        string GetRegistrationPassword();

        /// <summary>
        /// Limpia los campos del panel de registro.
        /// </summary>
        void ClearRegistrationFields();

        #endregion

        #region v2 - Panel Visibility

        /// <summary>
        /// Muestra el panel principal de login (email/contraseña).
        /// </summary>
        void ShowLoginPanel();

        /// <summary>
        /// Oculta el panel principal de login.
        /// </summary>
        void HideLoginPanel();

        #endregion
    }
}
