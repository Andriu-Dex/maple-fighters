using System;
using UI;

namespace Scripts.UI.Authenticator
{
    /// <summary>
    /// Interfaz para la vista de Login (patrón MVP).
    /// Define el contrato entre el Presenter y la Vista.
    /// </summary>
    public interface ILoginView : IView
    {
        /// <summary>
        /// Email ingresado por el usuario.
        /// </summary>
        string Email { get; set; }

        /// <summary>
        /// Contraseña ingresada por el usuario.
        /// </summary>
        string Password { get; set; }

        /// <summary>
        /// Se dispara cuando el usuario hace clic en Login.
        /// </summary>
        event Action<UIAuthenticationDetails> LoginButtonClicked;

        /// <summary>
        /// Se dispara cuando el usuario quiere crear una cuenta.
        /// </summary>
        event Action CreateAccountButtonClicked;

        /// <summary>
        /// Desactiva la interacción con la vista.
        /// </summary>
        void DisableInteraction();

        /// <summary>
        /// Reactiva la interacción con la vista.
        /// </summary>
        void EnableInteraction();
    }
}
