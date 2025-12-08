using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using UnityEngine;

namespace Scripts.Services.CharacterProviderApi
{
    using Random = UnityEngine.Random;

    /// <summary>
    /// API dummy para proveer personajes en modo offline/desarrollo.
    /// Refactorizado para usar ISaveService e IUserSession.
    /// </summary>
    public class DummyCharacterProviderApi : MonoBehaviour, ICharacterProviderApi
    {
        public static DummyCharacterProviderApi GetInstance()
        {
            if (instance == null)
            {
                var characterProviderApi =
                    new GameObject("Dummy CharacterProvider Api");
                instance =
                    characterProviderApi.AddComponent<DummyCharacterProviderApi>();
            }

            return instance;
        }

        private static DummyCharacterProviderApi instance;

        public Action<long, string> CreateCharacterCallback { get; set; }

        public Action<long, string> DeleteCharacterCallback { get; set; }

        public Action<long, string> GetCharactersCallback { get; set; }

        private Dictionary<int, CharacterData> characters;
        private const string StorageKey = "characters";
        
        private ISaveService saveService;

        private void Awake()
        {
            characters = new Dictionary<int, CharacterData>();
            
            // Intentar obtener el servicio de persistencia
            ServiceLocator.TryGet(out saveService);
        }

        private void OnDestroy()
        {
            // Limpiar la instancia singleton
            if (instance == this)
            {
                instance = null;
            }
            
            ApiProvider.RemoveCharacterProviderApi();
        }

        public void CreateCharacter(
            string userid,
            string charactername,
            int index,
            int classindex)
        {
            var statusCode = (long)StatusCodes.Created;
            var json = string.Empty;

            var id = GenerateId();
            var characterCollection = GetCharacterCollection();

            foreach (var character in characterCollection)
            {
                if (character.userid == userid &&
                    character.charactername == charactername)
                {
                    statusCode = (long)StatusCodes.BadRequest;
                    json = "Please choose a different character name.";

                    CreateCharacterCallback?.Invoke(statusCode, json);
                    return;
                }
            }

            characters.Add(id, new CharacterData()
            {
                id = id,
                userid = userid,
                characterlevel = 1,
                characterexperience = 0f,
                charactername = charactername,
                index = index,
                classindex = classindex
            });

            SaveCharacterCollection();

            CreateCharacterCallback?.Invoke(statusCode, json);
        }

        public void UpdateCharacter(int characterid, int characterlevel, float characterexperience)
        {
            if (characters.Count == 0)
            {
                // Usar IUserSession del ServiceLocator en lugar de FindObjectOfType
                var userId = GetCurrentUserId();
                if (!string.IsNullOrEmpty(userId))
                {
                    GetCharacters(userId);
                }
            }

            if (characters.TryGetValue(characterid, out CharacterData characterData))
            {
                characterData.characterlevel = characterlevel;
                characterData.characterexperience = characterexperience;
                characters[characterid] = characterData;
            }

            SaveCharacterCollection();
        }

        public void DeleteCharacter(int characterid)
        {
            var statusCode = (long)StatusCodes.Ok;
            var json = string.Empty;

            if (characters.Remove(characterid))
            {
                SaveCharacterCollection();

                DeleteCharacterCallback?.Invoke(statusCode, json);
            }
            else
            {
                statusCode = (long)StatusCodes.BadRequest;
                json = "The character was not found.";

                DeleteCharacterCallback?.Invoke(statusCode, json);
            }
        }

        public void GetCharacters(string userid)
        {
            var statusCode = (long)StatusCodes.Ok;
            var json = LoadCharactersJson();
            
            // Lista para almacenar solo los personajes de este usuario
            var filteredCharacters = new List<CharacterData>();

            var characterDataCollection = JsonUtility.FromJson<CharacterDataCollection>(json);
            if (characterDataCollection != null)
            {
                var characterItems = characterDataCollection.items;

                foreach (var character in characterItems)
                {
                    // FASE 2: Filtrar solo personajes de este usuario
                    if (character.userid != userid)
                    {
                        continue;
                    }
                    
                    // Agregar a la lista filtrada
                    filteredCharacters.Add(character);
                    
                    // Agregar al diccionario en memoria si no existe
                    if (!characters.ContainsKey(character.id))
                    {
                        characters.Add(character.id, new CharacterData()
                        {
                            id = character.id,
                            userid = character.userid,
                            charactername = character.charactername,
                            characterlevel = character.characterlevel,
                            characterexperience = character.characterexperience,
                            index = character.index,
                            classindex = character.classindex
                        });
                    }
                }
            }

            // Devolver JSON filtrado con solo los personajes de este usuario
            var filteredCollection = new CharacterDataCollection(filteredCharacters.ToArray());
            var filteredJson = filteredCollection.ToString();
            
            Debug.Log($"[DummyCharacterProviderApi] GetCharacters para userId={userid}: {filteredCharacters.Count} personajes encontrados");

            GetCharactersCallback?.Invoke(statusCode, filteredJson);
        }

        private void SaveCharacterCollection()
        {
            var characterCollection = GetCharacterCollection();
            var characterDataCollection = new CharacterDataCollection(characterCollection);
            var json = characterDataCollection.ToString();

            // Usar ISaveService si está disponible
            if (saveService != null)
            {
                saveService.SetString(StorageKey, json);
                saveService.Save();
            }
            else
            {
                // Fallback a PlayerPrefs
                PlayerPrefs.DeleteKey(StorageKey);
                PlayerPrefs.SetString(StorageKey, json);
                PlayerPrefs.Save();
            }
        }

        private string LoadCharactersJson()
        {
            // Usar ISaveService si está disponible
            if (saveService != null)
            {
                return saveService.GetString(StorageKey, string.Empty);
            }
            
            // Fallback a PlayerPrefs
            return PlayerPrefs.GetString(StorageKey, string.Empty);
        }

        private string GetCurrentUserId()
        {
            // Intentar obtener del ServiceLocator
            if (ServiceLocator.TryGet<IUserSession>(out var userSession))
            {
                return userSession.UserId;
            }
            
            // Fallback a FindObjectOfType (mantener compatibilidad)
            var userMetadata = FindObjectOfType<Scripts.Services.UserMetadata>();
            return userMetadata?.UserData?.id ?? string.Empty;
        }

        private CharacterData[] GetCharacterCollection()
        {
            return characters.Values.ToArray();
        }

        private int GenerateId()
        {
            return Random.Range(1, 10000);
        }
    }
}