using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.UI.Notice;

namespace Scripts.UI.Authenticator.Presenter
{
    /// <summary>
    /// Presenter para la pantalla de Registro (patrón MVP).
    /// Maneja la lógica de presentación entre la vista y el modelo.
    /// </summary>
    public class RegistrationPresenter : IDisposable
    {
        private readonly IRegistrationView view;
        private readonly IAuthenticationValidator validator;

        private Action<UIRegistrationDetails> onRegistrationRequested;
        private Action onBackRequested;

        public RegistrationPresenter(IRegistrationView view, IAuthenticationValidator validator)
        {
            this.view = view;
            this.validator = validator;

            SubscribeToViewEvents();
        }

        /// <summary>
        /// Evento que se dispara cuando se solicita un registro válido.
        /// </summary>
        public event Action<UIRegistrationDetails> RegistrationRequested
        {
            add => onRegistrationRequested += value;
            remove => onRegistrationRequested -= value;
        }

        /// <summary>
        /// Evento que se dispara cuando se solicita volver.
        /// </summary>
        public event Action BackRequested
        {
            add => onBackRequested += value;
            remove => onBackRequested -= value;
        }

        private void SubscribeToViewEvents()
        {
            if (view != null)
            {
                view.RegisterButtonClicked += OnRegisterButtonClicked;
                view.BackButtonClicked += OnBackButtonClicked;
            }
        }

        private void UnsubscribeFromViewEvents()
        {
            if (view != null)
            {
                view.RegisterButtonClicked -= OnRegisterButtonClicked;
                view.BackButtonClicked -= OnBackButtonClicked;
            }
        }

        private void OnRegisterButtonClicked(UIRegistrationDetails details)
        {
            var email = details.Email;
            var password = details.Password;
            var confirmPassword = details.ConfirmPassword;
            var firstName = details.FirstName;
            var lastName = details.LastName;

            if (!validator.ValidateRegistrationData(email, password, confirmPassword, firstName, lastName, out var errorMessage))
            {
                NoticeUtils.ShowNotice(errorMessage);
                return;
            }

            view?.DisableInteraction();
            onRegistrationRequested?.Invoke(details);
        }

        private void OnBackButtonClicked()
        {
            onBackRequested?.Invoke();
        }

        /// <summary>
        /// Llamar cuando el registro fue exitoso.
        /// </summary>
        public void OnRegistrationSucceeded()
        {
            view?.EnableInteraction();
            view?.Hide();
            ClearView();
        }

        /// <summary>
        /// Llamar cuando el registro falló.
        /// </summary>
        public void OnRegistrationFailed(string reason)
        {
            view?.EnableInteraction();
            NoticeUtils.ShowNotice(reason);
        }

        /// <summary>
        /// Muestra la vista de registro.
        /// </summary>
        public void Show()
        {
            view?.Show();
        }

        /// <summary>
        /// Oculta la vista de registro.
        /// </summary>
        public void Hide()
        {
            view?.Hide();
        }

        /// <summary>
        /// Limpia los campos de la vista.
        /// </summary>
        public void ClearView()
        {
            if (view != null)
            {
                view.Email = string.Empty;
                view.Password = string.Empty;
                view.ConfirmPassword = string.Empty;
                view.FirstName = string.Empty;
                view.LastName = string.Empty;
            }
        }

        public void Dispose()
        {
            UnsubscribeFromViewEvents();
        }
    }
}
