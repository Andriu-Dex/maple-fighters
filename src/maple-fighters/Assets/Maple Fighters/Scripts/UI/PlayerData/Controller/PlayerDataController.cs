using UI;
using UnityEngine;
using Scripts.Services;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;

namespace Scripts.UI.PlayerData
{
    /// <summary>
    /// Controlador para mostrar datos del jugador (nivel, experiencia, salud).
    /// Refactorizado para usar IUserSession del ServiceLocator.
    /// </summary>
    public class PlayerDataController : MonoBehaviour
    {
        private IPlayerDataView playerDataView;
        private IPlayerExperiencePointsView playerExperiencePointsView;

        private IUserSession userSession;

        private void Awake()
        {
            // Intentar obtener del ServiceLocator primero
            if (!ServiceLocator.TryGet(out userSession))
            {
                // Fallback a FindObjectOfType para compatibilidad
                var userMetadata = FindObjectOfType<UserMetadata>();
                userSession = userMetadata;
            }

            if (userSession != null)
            {
                userSession.CharacterExperiencePointsAdded += OnCharacterExperiencePointsAdded;
                userSession.CharacterLevelUp += OnCharacterLevelUp;
            }
        }

        private void Start()
        {
            CreatePlayerDataWindow();
            CreatePlayerExperiencePointsBar();
        }

        private void OnDestroy()
        {
            if (userSession != null)
            {
                userSession.CharacterExperiencePointsAdded -= OnCharacterExperiencePointsAdded;
                userSession.CharacterLevelUp -= OnCharacterLevelUp;
            }
        }

        public void SetPlayerHealth(int value)
        {
            playerDataView?.SetHealthPoints(value);
        }

        public void SetMaxPlayerHealth(int value)
        {
            playerDataView?.SetMaxHealthPoints(value);
        }

        private void CreatePlayerDataWindow()
        {
            if (userSession == null) return;

            playerDataView = UICreator
                .GetInstance()
                .Create<PlayerDataWindow>();
            playerDataView.SetLevel(userSession.CharacterLevel);
            playerDataView.SetName(userSession.CharacterName);
        }

        private void CreatePlayerExperiencePointsBar()
        {
            if (userSession == null) return;

            playerExperiencePointsView = UICreator
                .GetInstance()
                .Create<PlayerExperiencePointsBar>();
            playerExperiencePointsView.SetMaxExperiencePoints(userSession.GetMaxExperiencePoints());
            playerExperiencePointsView.SetExperiencePoints(userSession.GetExperiencePoints());
        }

        private void CreateLevelUpEffect()
        {
            var levelUpText = UICreator
                .GetInstance()
                .Create<LevelUpText>();
            levelUpText.Hide();
        }

        private void OnCharacterExperiencePointsAdded(float value)
        {
            playerExperiencePointsView?.AddExperiencePoints(value);
        }

        private void OnCharacterLevelUp(int value)
        {
            if (userSession == null) return;

            playerDataView?.SetLevel(userSession.CharacterLevel);

            playerExperiencePointsView?.SetMaxExperiencePoints(userSession.GetMaxExperiencePoints());
            playerExperiencePointsView?.SetExperiencePoints(userSession.GetExperiencePoints());

            CreateLevelUpEffect();
        }
    }
}