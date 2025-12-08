using System.Text.RegularExpressions;
using Scripts.Constants;
using Scripts.UI.Authenticator;
using Scripts.UI.GameServerBrowser;
using Scripts.UI.MenuBackground;
using Scripts.UI.ScreenFade;
using Scripts.UI.Notice;
using UI;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripts.UI.CharacterSelection
{
    [RequireComponent(typeof(CharacterViewInteractor))]
    public class CharacterViewController : MonoBehaviour,
                                           IOnCharacterReceivedListener,
                                           IOnCharacterDeletionFinishedListener,
                                           IOnCharacterCreationFinishedListener
    {
        [SerializeField]
        private bool showGameServerBrowser;

        [SerializeField]
        private int minCharacterNameLength;

        [Header("Startup Settings")]
        [SerializeField]
        [Tooltip("Si es false, no se muestra automáticamente al iniciar. Usar ShowCharacterSelection() para mostrar manualmente después del login.")]
        private bool showOnStart = false;

        private ILoadingView loadingView;
        private IChooseCharacterView chooseCharacterView;
        private ICharacterView characterView;
        private ICharacterSelectionOptionsView characterSelectionOptionsView;
        private ICharacterSelectionView characterSelectionView;
        private ICharacterNameView characterNameView;

        private CharacterViewCollection? characterViewCollection;
        private int characterIndex = -1;
        private UINewCharacterDetails characterDetails;

        private CharacterViewInteractor characterViewInteractor;
        private ScreenFadeController screenFadeController;
        
        /// <summary>
        /// Si true, usa flujo inteligente:
        /// - Sin personajes -> Ir directo a crear personaje
        /// - Con personajes -> Ir directo al juego con el primero
        /// </summary>
        private bool useSmartFlow = false;

        private void Awake()
        {
            characterViewInteractor = GetComponent<CharacterViewInteractor>();
            screenFadeController = FindObjectOfType<ScreenFadeController>();
        }

        private void Start()
        {
            // Si hay un PlayerLoginController o PlayerLoginIntegration en la escena,
            // NO iniciar automáticamente - esperar a que el login llame a ShowCharacterSelection()
            var loginController = FindObjectOfType<Scripts.UI.PlayerLogin.PlayerLoginController>();
            var loginIntegration = FindObjectOfType<Scripts.UI.PlayerLogin.PlayerLoginIntegration>();
            
            if (loginController != null || loginIntegration != null)
            {
                Debug.Log("[CharacterViewController] Sistema de login detectado, esperando login para iniciar...");
                return; // No auto-iniciar, esperar al login
            }
            
            // Solo auto-iniciar si showOnStart está activo Y no hay sistema de login
            if (showOnStart)
            {
                CreateAndShowCharacterView();
            }
        }

        /// <summary>
        /// Inicia el flujo de selección de personajes (modo clásico).
        /// Siempre muestra el menú de opciones (Choose/Create/Delete).
        /// </summary>
        public void ShowCharacterSelection()
        {
            useSmartFlow = false;
            CreateAndShowCharacterView();
        }
        
        /// <summary>
        /// Inicia el flujo inteligente de selección de personajes.
        /// FASE 3: Dirige automáticamente según si tiene o no personaje:
        /// - Sin personajes -> Va directo a crear personaje (selección de clase)
        /// - Con personajes -> Va directo al juego con el primer personaje
        /// </summary>
        public void ShowCharacterSelectionSmart()
        {
            useSmartFlow = true;
            Debug.Log("[CharacterViewController] Iniciando flujo inteligente...");
            CreateAndShowCharacterView();
        }

        private void OnDestroy()
        {
            UnsubscribeFromCharacterImages();
            UnsubscribeFromCharacterSelectionOptionsWindow();
            UnsubscribeFromCharacterSelectionWindow();
            UnsubscribeFromCharacterNameWindow();
            UnsubscribeFromBackgroundClicked();
        }

        private void CreateAndShowCharacterView()
        {
            CreateLoadingView();
            SubscribeToLoadingView();
            ShowLoadingView();
        }

        private void CreateChooseCharacterView()
        {
            chooseCharacterView = UICreator
                .GetInstance()
                .Create<ChooseCharacterText>(UICanvasLayer.Background, UIIndex.End);
        }

        private void CreateLoadingView()
        {
            loadingView = UICreator
                .GetInstance()
                .Create<LoadingText>(UICanvasLayer.Background, UIIndex.End);
        }

        private void CreateCharacterView()
        {
            characterView = UICreator
                .GetInstance()
                .Create<CharacterView>(UICanvasLayer.Background, UIIndex.End);
        }

        private void CreateAndSubscribeToCharacterSelectionOptionsWindow()
        {
            characterSelectionOptionsView = UICreator
                .GetInstance()
                .Create<CharacterSelectionOptionsWindow>(UICanvasLayer.Foreground, UIIndex.End);

            if (characterSelectionOptionsView != null)
            {
                characterSelectionOptionsView.ChooseCharacterButtonClicked +=
                    OnChooseCharacterButtonClicked;
                characterSelectionOptionsView.CreateCharacterButtonClicked +=
                    OnCreateCharacterButtonClicked;
                characterSelectionOptionsView.DeleteCharacterButtonClicked +=
                    OnDeleteCharacterButtonClicked;
            }
        }

        private void CreateAndSubscribeToCharacterSelectionWindow()
        {
            characterSelectionView = UICreator
                .GetInstance()
                .Create<CharacterSelectionWindow>(UICanvasLayer.Foreground, UIIndex.End);

            if (characterSelectionView != null)
            {
                characterSelectionView.CharacterSelected +=
                    OnCharacterSelected;
            }
        }

        private void CreateAndSubscribeToCharacterNameWindow()
        {
            characterNameView = UICreator
                .GetInstance()
                .Create<CharacterNameWindow>(UICanvasLayer.Foreground, UIIndex.End);

            if (characterNameView != null)
            {
                characterNameView.ConfirmButtonClicked +=
                    OnConfirmButtonClicked;
                characterNameView.BackButtonClicked +=
                    OnBackButtonClicked;
                characterNameView.NameInputFieldChanged +=
                    OnNameInputFieldChanged;
            }
        }

        private void SubscribeToLoadingView()
        {
            if (loadingView != null && loadingView.LoadingAnimation != null)
            {
                loadingView.LoadingAnimation.Finished +=
                    OnLoadingAnimationFinished;
            }
            else
            {
                Debug.LogWarning("[CharacterViewController] loadingView o LoadingAnimation es null, ejecutando flujo directo...");
                // Fallback: ejecutar directamente sin animación
                OnLoadingAnimationFinished();
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

        private void UnsubscribeFromCharacterImages()
        {
            var characterImages = characterViewCollection?.GetAll();
            if (characterImages != null)
            {
                foreach (var characterImage in characterImages)
                {
                    if (characterImage != null)
                    {
                        characterImage.CharacterClicked -=
                            OnCharacterClicked;
                    }
                }
            }
        }

        private void UnsubscribeFromCharacterSelectionOptionsWindow()
        {
            if (characterSelectionOptionsView != null)
            {
                characterSelectionOptionsView.ChooseCharacterButtonClicked -=
                    OnChooseCharacterButtonClicked;
                characterSelectionOptionsView.CreateCharacterButtonClicked -=
                    OnCreateCharacterButtonClicked;
                characterSelectionOptionsView.DeleteCharacterButtonClicked -=
                    OnDeleteCharacterButtonClicked;
            }
        }

        private void UnsubscribeFromCharacterSelectionWindow()
        {
            if (characterSelectionView != null)
            {
                characterSelectionView.CharacterSelected -=
                    OnCharacterSelected;
            }
        }

        private void UnsubscribeFromCharacterNameWindow()
        {
            if (characterNameView != null)
            {
                characterNameView.ConfirmButtonClicked -=
                    OnConfirmButtonClicked;
                characterNameView.BackButtonClicked -=
                    OnBackButtonClicked;
                characterNameView.NameInputFieldChanged -=
                    OnNameInputFieldChanged;
            }
        }

        private void UnsubscribeFromLoadingView()
        {
            if (loadingView != null && loadingView.LoadingAnimation != null)
            {
                loadingView.LoadingAnimation.Finished -=
                    OnLoadingAnimationFinished;
            }
        }

        public void OnCharacterReceived(UICharacterDetails characterDetails)
        {
            var path = Utils.GetCharacterPath(characterDetails);
            var characterView = CreateAndShowCharacterView(path);
            if (characterView != null)
            {
                characterView.Id = characterDetails.GetCharacterId();
                characterView.CharacterName = characterDetails.GetCharacterName();
                characterView.CharacterLevel = characterDetails.GetCharacterLevel();
                characterView.CharacterExperience = characterDetails.GetCharacterExperience();
                characterView.CharacterIndex = characterDetails.GetCharacterIndex();
                characterView.CharacterClass = characterDetails.GetCharacterClass();

                var characterIndex = characterDetails.GetCharacterIndex();
                if (characterIndex != UICharacterIndex.Zero)
                {
                    if (characterViewCollection == null)
                    {
                        var views =
                            new IClickableCharacterView[] { null, null, null };
                        characterViewCollection =
                            new CharacterViewCollection(views);
                    }

                    var index = (int)characterIndex;
                    characterViewCollection?.Set(index, characterView);
                }
            }
        }

        public void OnAfterCharacterReceived()
        {
            Debug.Log($"[CharacterViewController] OnAfterCharacterReceived - useSmartFlow={useSmartFlow}");
            
            HideLoadingView();
            
            // FASE 3: Flujo inteligente
            if (useSmartFlow)
            {
                HandleSmartFlow();
                return;
            }
            
            // Flujo clásico: mostrar menú de opciones
            ShowChooseCharacterView();
        }
        
        /// <summary>
        /// FASE 3: Maneja el flujo inteligente post-login.
        /// </summary>
        private void HandleSmartFlow()
        {
            // Verificar si tiene personajes
            bool hasCharacter = HasAnyCharacter();
            
            Debug.Log($"[CharacterViewController] HandleSmartFlow - hasCharacter={hasCharacter}");
            
            if (hasCharacter)
            {
                // Usuario tiene personaje -> Ir directo al juego con el primero
                Debug.Log("[CharacterViewController] Flujo inteligente: Usuario tiene personaje, yendo al juego...");
                GoToGameWithFirstCharacter();
            }
            else
            {
                // Usuario no tiene personaje -> Ir directo a crear
                Debug.Log("[CharacterViewController] Flujo inteligente: Usuario nuevo, yendo a crear personaje...");
                GoToCreateCharacter();
            }
        }
        
        /// <summary>
        /// Verifica si hay algún personaje VÁLIDO en la colección.
        /// Un personaje válido debe tener ID > 0 y nombre no vacío.
        /// </summary>
        private bool HasAnyCharacter()
        {
            if (characterViewCollection == null)
            {
                Debug.Log("[CharacterViewController] HasAnyCharacter: characterViewCollection es null");
                return false;
            }
            
            var characters = characterViewCollection.Value.GetAll();
            if (characters == null)
            {
                Debug.Log("[CharacterViewController] HasAnyCharacter: GetAll() retornó null");
                return false;
            }
            
            foreach (var character in characters)
            {
                // Verificar que sea un personaje VÁLIDO (no solo que exista el objeto)
                if (character != null && 
                    character.Id > 0 && 
                    !string.IsNullOrEmpty(character.CharacterName))
                {
                    Debug.Log($"[CharacterViewController] HasAnyCharacter: Encontrado personaje válido - ID:{character.Id}, Name:{character.CharacterName}");
                    return true;
                }
            }
            
            Debug.Log("[CharacterViewController] HasAnyCharacter: No hay personajes válidos en la colección");
            return false;
        }
        
        /// <summary>
        /// Va directo al juego con el primer personaje disponible.
        /// Para usuarios que ya tienen personaje.
        /// </summary>
        private void GoToGameWithFirstCharacter()
        {
            // Buscar el primer personaje disponible
            if (characterViewCollection == null)
            {
                GoToCreateCharacter();
                return;
            }
            
            var characters = characterViewCollection.Value.GetAll();
            if (characters != null)
            {
                int index = 0;
                foreach (var character in characters)
                {
                    // Verificar que sea un personaje VÁLIDO (ID > 0, nombre no vacío)
                    if (character != null && 
                        character.Id > 0 && 
                        !string.IsNullOrEmpty(character.CharacterName))
                    {
                        characterIndex = index;
                        
                        var characterId = character.Id;
                        var characterClass = (byte)character.CharacterClass;
                        var characterName = character.CharacterName;
                        var characterLevel = character.CharacterLevel;
                        var characterExperience = character.CharacterExperience;

                        characterViewInteractor.UpdateCharacterData(characterId, characterClass, characterName, characterLevel, characterExperience);

                        Debug.Log($"[CharacterViewController] Auto-seleccionando personaje: {characterName} (ID: {characterId})");

                        if (showGameServerBrowser)
                        {
                            ShowGameServerBrowserWindow();
                        }
                        else
                        {
                            LoadLobby();
                        }
                        
                        return;
                    }
                    index++;
                }
            }
            
            // Si no encontró personaje válido, ir a crear
            Debug.LogWarning("[CharacterViewController] No se encontró personaje válido, redirigiendo a crear...");
            GoToCreateCharacter();
        }
        
        /// <summary>
        /// Va directo a la pantalla de selección de clase para crear personaje.
        /// Para usuarios nuevos sin personaje.
        /// </summary>
        private void GoToCreateCharacter()
        {
            // Usar el primer slot disponible
            characterIndex = 0;
            
            ShowChooseCharacterView();
            ShowCharacterSelectionWindow();
            
            Debug.Log("[CharacterViewController] Mostrando selección de clase para crear personaje");
        }

        public void OnCharacterDeletionSucceed()
        {
            HideChooseCharacterView();
            ShowLoadingView();

            LoadCharacters();
        }

        public void OnCharacterDeletionFailed()
        {
            NoticeUtils.ShowNotice(message: NoticeMessages.CharacterView.DeletionFailed);
        }

        public void OnCharacterCreated()
        {
            ResetCharacterName();

            HideCharacterNameWindow();
            HideChooseCharacterView();

            ShowLoadingView();

            LoadCharacters();
        }

        public void OnCreateCharacterFailed(UICharacterCreationFailed reason)
        {
            switch (reason)
            {
                case UICharacterCreationFailed.Unknown:
                {
                    NoticeUtils.ShowNotice(message: NoticeMessages.CharacterView.CreationFailed);
                    break;
                }

                case UICharacterCreationFailed.NameAlreadyInUse:
                {
                    NoticeUtils.ShowNotice(message: NoticeMessages.CharacterView.NameAlreadyInUse);
                    break;
                }
            }

            characterNameView.EnableConfirmButton();
        }

        private void OnLoadingAnimationFinished()
        {
            Debug.Log("[CharacterViewController] OnLoadingAnimationFinished - Creando vistas y cargando personajes...");
            
            UnsubscribeFromLoadingView();

            CreateChooseCharacterView();
            CreateCharacterView();
            CreateAndSubscribeToCharacterSelectionOptionsWindow();
            CreateAndSubscribeToCharacterSelectionWindow();
            CreateAndSubscribeToCharacterNameWindow();

            SubscribeToBackgroundClicked();

            LoadCharacters();
        }

        public void LoadCharacters()
        {
            Debug.Log("[CharacterViewController] LoadCharacters - Solicitando personajes del servidor...");
            
            RemoveAllCharacterImages();
            
            // Limpiar la colección de personajes para evitar datos residuales
            characterViewCollection = null;

            characterViewInteractor.GetCharacters();
        }

        private void RemoveAllCharacterImages()
        {
            var characterImages = characterViewCollection?.GetAll();
            if (characterImages != null)
            {
                foreach (var characterImage in characterImages)
                {
                    if (characterImage != null)
                    {
                        var gameObject = characterImage.GameObject;
                        if (gameObject != null)
                        {
                            Destroy(gameObject);
                        }
                    }
                }
            }
        }

        private void ShowChooseCharacterView()
        {
            chooseCharacterView?.Show();
        }

        private void ShowLoadingView()
        {
            loadingView?.Show();
        }

        private void HideChooseCharacterView()
        {
            chooseCharacterView?.Hide();
        }

        private void HideLoadingView()
        {
            loadingView?.Hide();
        }

        private void ShowCharacterSelectionOptionsWindow()
        {
            characterSelectionOptionsView?.Show();
        }

        public void HideCharacterSelectionOptionsWindow()
        {
            if (characterSelectionOptionsView != null &&
                characterSelectionOptionsView.IsShown)
            {
                characterSelectionOptionsView.Hide();
            }
        }

        private void EnableOrDisableCharacterSelectionOptionsViewButtons(bool hasCharacter)
        {
            characterSelectionOptionsView?.EnableOrDisableChooseCharacterButton(hasCharacter);
            characterSelectionOptionsView?.EnableOrDisableCreateCharacterButton(!hasCharacter);
            characterSelectionOptionsView?.EnableOrDisableDeleteCharacterButton(hasCharacter);
        }

        private void OnCharacterClicked(UICharacterIndex uiCharacterIndex, bool hasCharacter)
        {
            characterIndex = (int)uiCharacterIndex;

            HideCharacterNameWindow();
            HideCharacterSelectionWindow();
            HideChooseCharacterView();
            HideGameServerBrowserWindow();
            HideAuthenticatorView();

            ShowCharacterSelectionOptionsWindow();

            EnableOrDisableCharacterSelectionOptionsViewButtons(hasCharacter);
        }

        private void OnChooseCharacterButtonClicked()
        {
            if (characterIndex != -1)
            {
                var character = characterViewCollection?.Get(characterIndex);
                if (character != null)
                {
                    var characterId = character.Id;
                    var characterClass = (byte)character.CharacterClass;
                    var characterName = character.CharacterName;
                    var characterLevel = character.CharacterLevel;
                    var characterExperience = character.CharacterExperience;

                    characterViewInteractor.UpdateCharacterData(characterId, characterClass, characterName, characterLevel, characterExperience);

                    HideCharacterSelectionOptionsWindow();

                    if (showGameServerBrowser)
                    {
                        ShowGameServerBrowserWindow();
                    }
                    else
                    {
                        LoadLobby();
                    }
                }
                else
                {
                    NoticeUtils.ShowNotice(message: NoticeMessages.CharacterView.NotFound);
                }
            }
        }

        private void LoadLobby()
        {
            if (screenFadeController != null)
            {
                screenFadeController.Show();
                screenFadeController.FadeInCompleted += OnFadeInCompleted;
            }
        }

        private void OnFadeInCompleted()
        {
            if (screenFadeController != null)
            {
                screenFadeController.FadeInCompleted -= OnFadeInCompleted;
            }

            var mapName = Constants.SceneNames.Maps.Lobby;

            SceneManager.LoadScene(sceneName: mapName);
        }

        private void OnCreateCharacterButtonClicked()
        {
            HideCharacterSelectionOptionsWindow();
            ShowCharacterSelectionWindow();
        }

        private void OnDeleteCharacterButtonClicked()
        {
            HideCharacterSelectionOptionsWindow();

            if (characterIndex != -1)
            {
                var character = characterViewCollection?.Get(characterIndex);
                if (character != null)
                {
                    var characterId = character.Id;
                    characterViewInteractor.RemoveCharacter(characterId);
                }
            }
        }

        private void OnNameInputFieldChanged(string characterName)
        {
            if (IsCharacterNameValid(characterName))
            {
                characterNameView?.EnableConfirmButton();
            }
            else
            {
                characterNameView?.DisableConfirmButton();
            }
        }

        private bool IsCharacterNameValid(string name)
        {
            var isEnoughLength = name.Length >= minCharacterNameLength;
            var isNotEmpty = !string.IsNullOrEmpty(name);
            var isNotWhitespace = !Regex.IsMatch(name, @"\s");

            return isEnoughLength && isNotEmpty && isNotWhitespace;
        }

        private void OnConfirmButtonClicked(string characterName)
        {
            characterDetails.SetCharacterName(characterName);
            characterViewInteractor.CreateCharacter(characterIndex, characterDetails);
        }

        private void OnBackButtonClicked()
        {
            HideCharacterNameWindow();
            ShowCharacterSelectionWindow();
        }

        private void OnCharacterSelected(UICharacterClass uiCharacterClass)
        {
            characterDetails.SetCharacterClass(uiCharacterClass);

            HideCharacterSelectionWindow();
            ShowCharacterNameWindow();
        }

        private void HideAuthenticatorView()
        {
            var authenticatorController = FindObjectOfType<AuthenticatorController>();
            authenticatorController?.HideLoginWindow();
            authenticatorController?.HideRegistrationWindow();
        }

        private void ResetCharacterName()
        {
            characterNameView.ResetNameInputField();
        }

        private void OnBackgroundClicked()
        {
            HideCharacterSelectionOptionsWindow();
            HideCharacterSelectionWindow();
            HideCharacterNameWindow();

            ShowChooseCharacterView();
        }

        private void ShowCharacterNameWindow()
        {
            characterNameView?.Show();
            characterNameView?.GenerateRandomCharacterName();
        }

        public void HideCharacterNameWindow()
        {
            if (characterNameView != null &&
                characterNameView.IsShown)
            {
                characterNameView?.Hide();
            }
        }

        private void ShowCharacterSelectionWindow()
        {
            characterSelectionView?.Show();
        }

        public void HideCharacterSelectionWindow()
        {
            if (characterSelectionView != null &&
                characterSelectionView.IsShown)
            {
                characterSelectionView?.Hide();
            }
        }

        private void ShowGameServerBrowserWindow()
        {
            var gameServerBrowserController =
                FindObjectOfType<GameServerBrowserController>();
            gameServerBrowserController?.ShowGameServerBrowserWindow();
        }

        private void HideGameServerBrowserWindow()
        {
            var gameServerBrowserController =
                FindObjectOfType<GameServerBrowserController>();
            gameServerBrowserController?.HideGameServerBrowserWindow();
        }

        private IClickableCharacterView CreateAndShowCharacterView(string path)
        {
            IClickableCharacterView characterView = null;

            var character = CreateCharacterView(path);
            if (character != null)
            {
                characterView =
                    character.GetComponent<ClickableCharacterImage>();

                if (characterView != null)
                {
                    characterView.CharacterClicked += OnCharacterClicked;
                    characterView.Show();
                }
            }

            return characterView;
        }

        private GameObject CreateCharacterView(string path)
        {
            var characterPrefab = Resources.Load<GameObject>(path);
            var character = Instantiate(characterPrefab);
            if (character != null)
            {
                if (characterView != null)
                {
                    var view = characterView.Transform;

                    character.transform.SetParent(view, false);
                    character.transform.SetAsLastSibling();
                }
            }

            return character;
        }
    }
}