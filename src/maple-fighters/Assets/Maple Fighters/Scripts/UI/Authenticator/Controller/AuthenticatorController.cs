using Scripts.Constants;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.UI.CharacterSelection;
using Scripts.UI.GameServerBrowser;
using Scripts.UI.MenuBackground;
using Scripts.UI.Notice;
using UI;
using UnityEngine;

namespace Scripts.UI.Authenticator
{
    [RequireComponent(typeof(AuthenticatorInteractor))]
    public class AuthenticatorController : MonoBehaviour,
                                           IOnLoginFinishedListener,
                                           IOnRegistrationFinishedListener
    {
        private ILoginView loginView;
        private IRegistrationView registrationView;

        private IAuthenticationValidator authenticationValidator;
        private AuthenticatorInteractor authenticatorInteractor;

        private void Awake()
        {
            // Intentar obtener el validador del ServiceLocator, con fallback
            if (!ServiceLocator.TryGet(out authenticationValidator))
            {
                authenticationValidator = new AuthenticationValidator();
            }
            
            authenticatorInteractor = GetComponent<AuthenticatorInteractor>();
        }

        private void Start()
        {
            CreateAndSubscribeToLoginWindow();
            CreateAndSubscribeToRegistrationWindow();

            SubscribeToBackgroundClicked();

#if UNITY_EDITOR
            CreateLoginButton();
#endif
        }

        private void CreateLoginButton()
        {
#if UNITY_EDITOR
            var loginButton = UICreator.GetInstance().Create<LoginButton>();
            if (loginButton != null)
            {
                loginButton.ButtonClicked += ShowLoginWindow;
            }
#endif
        }

        private void CreateAndSubscribeToLoginWindow()
        {
            loginView = UICreator
                .GetInstance()
                .Create<LoginWindow>();

            if (loginView != null)
            {
                loginView.Email =
                    ProvideLoginEmail();
            }

            if (loginView != null)
            {
                loginView.LoginButtonClicked +=
                    OnLoginButtonClicked;
                loginView.CreateAccountButtonClicked +=
                    OnCreateAccountButtonClicked;
            }
        }

        private void CreateAndSubscribeToRegistrationWindow()
        {
            registrationView = UICreator
                .GetInstance()
                .Create<RegistrationWindow>();

            if (registrationView != null)
            {
                registrationView.RegisterButtonClicked +=
                    OnRegisterButtonClicked;
                registrationView.BackButtonClicked +=
                    OnBackButtonClicked;
            }
        }

        private void OnDestroy()
        {
            UnsubscribeFromLoginWindow();
            UnsubscribeFromRegistrationWindow();
            UnsubscribeFromBackgroundClicked();
        }

        private void UnsubscribeFromLoginWindow()
        {
            if (loginView != null)
            {
                loginView.LoginButtonClicked -=
                    OnLoginButtonClicked;
                loginView.CreateAccountButtonClicked -=
                    OnCreateAccountButtonClicked;
            }
        }

        private void UnsubscribeFromRegistrationWindow()
        {
            if (registrationView != null)
            {
                registrationView.RegisterButtonClicked -=
                    OnRegisterButtonClicked;
                registrationView.BackButtonClicked -=
                    OnBackButtonClicked;
            }
        }

        private void SubscribeToBackgroundClicked()
        {
            var backgroundController =
                FindObjectOfType<MenuBackgroundController>();
            if (backgroundController != null)
            {
                backgroundController.BackgroundClicked +=
                    OnBackgroundClicked;
            }
        }

        private void UnsubscribeFromBackgroundClicked()
        {
            var backgroundController =
                FindObjectOfType<MenuBackgroundController>();
            if (backgroundController != null)
            {
                backgroundController.BackgroundClicked -=
                    OnBackgroundClicked;
            }
        }

        private void OnBackgroundClicked()
        {
            HideLoginWindow();
            HideRegistrationWindow();
        }

        private void OnLoginButtonClicked(
            UIAuthenticationDetails uiAuthenticationDetails)
        {
            var email = uiAuthenticationDetails.Email;
            var password = uiAuthenticationDetails.Password;

            // Usar el método consolidado de validación
            if (!authenticationValidator.ValidateLoginData(email, password, out var message))
            {
                NoticeUtils.ShowNotice(message);
            }
            else
            {
                loginView?.DisableInteraction();
                authenticatorInteractor.Login(uiAuthenticationDetails);
            }
        }

        private void OnCreateAccountButtonClicked()
        {
            HideLoginWindow();
            ShowRegistrationWindow();
        }

        private void OnRegisterButtonClicked(
            UIRegistrationDetails uiRegistrationDetails)
        {
            var email = uiRegistrationDetails.Email;
            var password = uiRegistrationDetails.Password;
            var confirmPassword = uiRegistrationDetails.ConfirmPassword;
            var firstName = uiRegistrationDetails.FirstName;
            var lastName = uiRegistrationDetails.LastName;

            // Usar el método consolidado de validación
            if (!authenticationValidator.ValidateRegistrationData(email, password, confirmPassword, firstName, lastName, out var message))
            {
                NoticeUtils.ShowNotice(message);
            }
            else
            {
                registrationView?.DisableInteraction();
                authenticatorInteractor.Register(uiRegistrationDetails);
            }
        }

        private void OnBackButtonClicked()
        {
            HideRegistrationWindow();
            ShowLoginWindow();
        }

        private void ShowLoginWindow()
        {
            HideLoginWindow();
            HideRegistrationWindow();
            HideCharacterView();
            HideGameServerBrowserView();

            loginView?.Show();
        }

        private void HideCharacterView()
        {
            var characterView = FindObjectOfType<CharacterViewController>();
            characterView?.HideCharacterSelectionOptionsWindow();
            characterView?.HideCharacterSelectionWindow();
            characterView?.HideCharacterNameWindow();
        }

        private void HideGameServerBrowserView()
        {
            var gameServerBrowser = FindObjectOfType<GameServerBrowserController>();
            gameServerBrowser?.HideGameServerBrowserWindow();
        }

        public void HideLoginWindow()
        {
            if (loginView != null &&
                loginView.IsShown)
            {
                loginView.Hide();
            }
        }

        private void ShowRegistrationWindow()
        {
            registrationView?.Show();
        }

        public void HideRegistrationWindow()
        {
            if (registrationView != null &&
                registrationView.IsShown)
            {
                registrationView.Hide();
            }
        }

        public void OnLoginSucceeded()
        {
            var email = loginView?.Email;

            loginView?.Hide();
            loginView?.EnableInteraction();

            SaveLoginEmail(email);
            ClearLoginWindow();

            var characterViewInteractor =
                FindObjectOfType<CharacterViewInteractor>();
            var characterViewController =
                FindObjectOfType<CharacterViewController>();

            characterViewInteractor?.SetCharacterProviderApi();
            characterViewController?.LoadCharacters();
        }

        public void OnLoginFailed(string reason)
        {
            loginView?.EnableInteraction();

            NoticeUtils.ShowNotice(message: reason);
        }

        public void OnRegistrationSucceeded()
        {
            registrationView?.EnableInteraction();

            ClearRegistrationWindow();
            HideRegistrationWindow();
            ShowLoginWindow();

            NoticeUtils.ShowNotice(message: NoticeMessages.AuthView.RegistrationSucceed);
        }

        public void OnRegistrationFailed(string reason)
        {
            registrationView?.EnableInteraction();

            NoticeUtils.ShowNotice(message: reason);
        }

        private void ClearLoginWindow()
        {
            if (loginView != null)
            {
                loginView.Email = string.Empty;
                loginView.Password = string.Empty;
            }
        }

        private void ClearRegistrationWindow()
        {
            if (registrationView != null)
            {
                registrationView.Email = string.Empty;
                registrationView.Password = string.Empty;
                registrationView.ConfirmPassword = string.Empty;
                registrationView.FirstName = string.Empty;
                registrationView.LastName = string.Empty;
            }
        }

        private void SaveLoginEmail(string email)
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                saveService.SetString(SaveKeys.Email, email);
                saveService.Save();
            }
            else
            {
                // Fallback a PlayerPrefs si el servicio no está disponible
                PlayerPrefs.SetString(SaveKeys.Email, email);
                PlayerPrefs.Save();
            }
        }

        private string ProvideLoginEmail()
        {
            var saveService = ServiceLocator.Get<ISaveService>();
            if (saveService != null)
            {
                return saveService.GetString(SaveKeys.Email, string.Empty);
            }
            
            // Fallback a PlayerPrefs si el servicio no está disponible
            return PlayerPrefs.HasKey(SaveKeys.Email) 
                ? PlayerPrefs.GetString(SaveKeys.Email) 
                : string.Empty;
        }

        /// <summary>
        /// Claves de guardado para evitar strings mágicos.
        /// </summary>
        private static class SaveKeys
        {
            public const string Email = "user_email";
        }
    }
}