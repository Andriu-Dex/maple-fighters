using UnityEngine;
using Scripts.Services.AuthenticatorApi;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using System.Text.RegularExpressions;
using System;

namespace Scripts.Services
{
    /// <summary>
    /// Metadatos del usuario y su personaje actual.
    /// Implementa IUserSession para desacoplamiento.
    /// </summary>
    public class UserMetadata : MonoBehaviour, IUserSession
    {
        public event Action<float> CharacterExperiencePointsAdded;

        public event Action<int> CharacterLevelUp;

        public UserData UserData { get; set; }

        // IUserSession.UserId
        public string UserId => UserData?.id ?? string.Empty;

        public int CharacterId { get; set; }

        public int CharacterType { get; set; }

        public int CharacterHealth { get; set; }

        public int CharacterLevel { get; set; }

        public string CharacterName { get; set; }

        public float CharacterExperiencePoints { get; set; }

        public bool IsLoggedIn { get; set; }

        private ISaveService saveService;
        private const string UserIdKey = "userid";

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
            // Intentar obtener el servicio de persistencia
            ServiceLocator.TryGet(out saveService);
        }

        private void Start()
        {
            UserData = new UserData()
            {
                id = GetUserId()
            };

            CharacterHealth = GetMaxCharacterHealth();
            
            // Registrar en ServiceLocator para que otros puedan acceder
            RegisterInServiceLocator();
        }

        private void RegisterInServiceLocator()
        {
            // Registrar como IUserSession si no está ya registrado
            if (!ServiceLocator.TryGet<IUserSession>(out _))
            {
                ServiceLocator.Register<IUserSession>(this);
                Debug.Log("[UserMetadata] Registered as IUserSession in ServiceLocator");
            }
        }

        public void AddExperiencePoints(float value)
        {
            CharacterExperiencePoints += value;

            VerifyCharacterLevel(value);
            SaveCharacterData();
        }

        public int GetMaxCharacterHealth()
        {
            return 500;
        }

        public float GetExperiencePoints()
        {
            return CharacterExperiencePoints;
        }

        public float GetMaxExperiencePoints()
        {
            return CharacterLevel * 100;
        }

        private void VerifyCharacterLevel(float value)
        {
            if (CharacterExperiencePoints >= GetMaxExperiencePoints())
            {
                CharacterExperiencePoints = 0;
                CharacterLevel++;

                CharacterLevelUp?.Invoke(CharacterLevel);
            }
            else
            {
                CharacterExperiencePointsAdded?.Invoke(value);
            }
        }

        private void SaveCharacterData()
        {
            var characterProviderApi = ApiProvider.ProvideCharacterProviderApi();
            characterProviderApi.UpdateCharacter(CharacterId, CharacterLevel, CharacterExperiencePoints);
        }

        private string GetUserId()
        {
            string userid;

            // Usar ISaveService si está disponible
            if (saveService != null)
            {
                if (saveService.HasKey(UserIdKey))
                {
                    userid = saveService.GetString(UserIdKey);
                }
                else
                {
                    userid = GenerateUserId();
                    saveService.SetString(UserIdKey, userid);
                    saveService.Save();
                }
            }
            else
            {
                // Fallback a PlayerPrefs
                if (PlayerPrefs.HasKey(UserIdKey))
                {
                    userid = PlayerPrefs.GetString(UserIdKey);
                }
                else
                {
                    userid = GenerateUserId();
                    PlayerPrefs.SetString(UserIdKey, userid);
                    PlayerPrefs.Save();
                }
            }

            return userid;
        }

        private string GenerateUserId()
        {
            var userid = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            return Regex.Replace(userid, "[/+=]", string.Empty);
        }
    }
}