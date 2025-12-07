using System;
using UI;

namespace Scripts.UI.PlayerLogin
{
    /// <summary>
    /// Interface para la vista de login de jugadores.
    /// Sigue el patrón MVP (Model-View-Presenter).
    /// </summary>
    public interface IPlayerLoginView : IView
    {
        /// <summary>
        /// Se dispara cuando el usuario confirma la entrada (nombre o contraseña).
        /// Parámetro: texto ingresado.
        /// </summary>
        event Action<string> ConfirmButtonClicked;

        /// <summary>
        /// Se dispara cuando el usuario presiona el botón de volver.
        /// </summary>
        event Action BackButtonClicked;

        /// <summary>
        /// Se dispara cuando cambia el texto del input.
        /// </summary>
        event Action<string> InputFieldChanged;

        /// <summary>
        /// Texto actual del campo de entrada.
        /// </summary>
        string InputText { get; set; }

        /// <summary>
        /// Título de la ventana (para cambiar dinámicamente).
        /// </summary>
        string Title { set; }

        /// <summary>
        /// Texto del placeholder del input.
        /// </summary>
        string Placeholder { set; }

        /// <summary>
        /// Mensaje de estado/error para mostrar al usuario.
        /// </summary>
        string StatusMessage { set; }

        /// <summary>
        /// Habilita el botón de confirmar.
        /// </summary>
        void EnableConfirmButton();

        /// <summary>
        /// Deshabilita el botón de confirmar.
        /// </summary>
        void DisableConfirmButton();

        /// <summary>
        /// Habilita toda la interacción con la vista.
        /// </summary>
        void EnableInteraction();

        /// <summary>
        /// Deshabilita toda la interacción con la vista.
        /// </summary>
        void DisableInteraction();

        /// <summary>
        /// Configura el input como campo de contraseña (oculta caracteres).
        /// </summary>
        void SetPasswordMode(bool isPassword);

        /// <summary>
        /// Limpia el campo de entrada.
        /// </summary>
        void ClearInput();

        /// <summary>
        /// Enfoca el campo de entrada.
        /// </summary>
        void FocusInput();
    }
}
