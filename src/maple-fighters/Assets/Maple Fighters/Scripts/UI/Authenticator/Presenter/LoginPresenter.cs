using System;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.UI.Notice;

namespace Scripts.UI.Authenticator.Presenter
{
    /// <summary>
    /// Presenter para la pantalla de Login (patrón MVP).
    /// Maneja la lógica de presentación entre la vista y el modelo.
    /// </summary>
    public class LoginPresenter : IDisposable
    {
        private readonly ILoginView view;
        private readonly IAuthenticationValidator validator;
        private readonly ISaveService saveService;

        private Action<UIAuthenticationDetails> onLoginRequested;
        private Action onCreateAccountRequested;

        private const string EmailSaveKey = "user_email";

        public LoginPresenter(ILoginView view, IAuthenticationValidator validator)
        {
            this.view = view;
            this.validator = validator;
            
            // Intentar obtener el servicio de persistencia
            ServiceLocator.TryGet(out saveService);

            SubscribeToViewEvents();
            LoadSavedEmail();
        }

        /// <summary>
        /// Evento que se dispara cuando se solicita un login válido.
        /// </summary>
        public event Action<UIAuthenticationDetails> LoginRequested
        {
            add => onLoginRequested += value;
            remove => onLoginRequested -= value;
        }

        /// <summary>
        /// Evento que se dispara cuando se solicita crear una cuenta.
        /// </summary>
        public event Action CreateAccountRequested
        {
            add => onCreateAccountRequested += value;
            remove => onCreateAccountRequested -= value;
        }

        private void SubscribeToViewEvents()
        {
            if (view != null)
            {
                view.LoginButtonClicked += OnLoginButtonClicked;
                view.CreateAccountButtonClicked += OnCreateAccountButtonClicked;
            }
        }

        private void UnsubscribeFromViewEvents()
        {
            if (view != null)
            {
                view.LoginButtonClicked -= OnLoginButtonClicked;
                view.CreateAccountButtonClicked -= OnCreateAccountButtonClicked;
            }
        }

        private void LoadSavedEmail()
        {
            if (view == null) return;

            var savedEmail = saveService?.GetString(EmailSaveKey, string.Empty) ?? string.Empty;
            if (!string.IsNullOrEmpty(savedEmail))
            {
                view.Email = savedEmail;
            }
        }

        private void OnLoginButtonClicked(UIAuthenticationDetails details)
        {
            var email = details.Email;
            var password = details.Password;

            if (!validator.ValidateLoginData(email, password, out var errorMessage))
            {
                NoticeUtils.ShowNotice(errorMessage);
                return;
            }

            view?.DisableInteraction();
            onLoginRequested?.Invoke(details);
        }

        private void OnCreateAccountButtonClicked()
        {
            onCreateAccountRequested?.Invoke();
        }

        /// <summary>
        /// Llamar cuando el login fue exitoso.
        /// </summary>
        public void OnLoginSucceeded()
        {
            var email = view?.Email;

            // Guardar email para próxima sesión
            if (!string.IsNullOrEmpty(email))
            {
                saveService?.SetString(EmailSaveKey, email);
                saveService?.Save();
            }

            view?.EnableInteraction();
            view?.Hide();
            ClearView();
        }

        /// <summary>
        /// Llamar cuando el login falló.
        /// </summary>
        public void OnLoginFailed(string reason)
        {
            view?.EnableInteraction();
            NoticeUtils.ShowNotice(reason);
        }

        /// <summary>
        /// Muestra la vista de login.
        /// </summary>
        public void Show()
        {
            view?.Show();
        }

        /// <summary>
        /// Oculta la vista de login.
        /// </summary>
        public void Hide()
        {
            if (view != null && view.IsShown)
            {
                view.Hide();
            }
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
            }
        }

        public void Dispose()
        {
            UnsubscribeFromViewEvents();
        }
    }
}
