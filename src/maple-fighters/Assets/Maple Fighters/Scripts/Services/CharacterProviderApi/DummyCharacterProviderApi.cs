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
            Debug.Log($"[DummyCharacterProviderApi] UpdateCharacter: id={characterid}, level={characterlevel}, exp={characterexperience}");
            
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
                Debug.Log($"[DummyCharacterProviderApi] Personaje actualizado en memoria: {characterData.charactername} nivel={characterlevel}");
            }
            else
            {
                Debug.LogWarning($"[DummyCharacterProviderApi] No se encontró personaje con id={characterid} en memoria. characters.Count={characters.Count}");
            }

            SaveCharacterCollection();
        }

        public void DeleteCharacter(int characterid)
        {
            var statusCode = (long)StatusCodes.Ok;
            var json = string.Empty;

            // Intentar eliminar de memoria primero
            bool removed = characters.Remove(characterid);
            
            // Si no estaba en memoria, intentar eliminar directamente del archivo
            if (!removed)
            {
                var existingJson = LoadCharactersJson();
                if (!string.IsNullOrEmpty(existingJson))
                {
                    var existingCollection = JsonUtility.FromJson<CharacterDataCollection>(existingJson);
                    if (existingCollection?.items != null)
                    {
                        var updatedList = existingCollection.items.Where(c => c.id != characterid).ToArray();
                        if (updatedList.Length < existingCollection.items.Length)
                        {
                            removed = true;
                            // Guardar directamente sin pasar por SaveCharacterCollection
                            var updatedCollection = new CharacterDataCollection(updatedList);
                            var updatedJson = updatedCollection.ToString();
                            if (saveService != null)
                            {
                                saveService.SetString(StorageKey, updatedJson);
                                saveService.Save();
                            }
                            else
                            {
                                PlayerPrefs.DeleteKey(StorageKey);
                                PlayerPrefs.SetString(StorageKey, updatedJson);
                                PlayerPrefs.Save();
                            }
                        }
                    }
                }
            }
            else
            {
                // Si se eliminó de memoria, guardar normalmente (merge)
                SaveCharacterCollection();
            }

            if (removed)
            {
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
            
            Debug.Log($"[DummyCharacterProviderApi] GetCharacters para userId={userid}, JSON cargado: {json}");
            
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
                    
                    // SIEMPRE actualizar el diccionario en memoria con los datos del archivo
                    // Esto asegura que al recargar, tengamos los datos más recientes
                    var characterData = new CharacterData()
                    {
                        id = character.id,
                        userid = character.userid,
                        charactername = character.charactername,
                        characterlevel = character.characterlevel,
                        characterexperience = character.characterexperience,
                        index = character.index,
                        classindex = character.classindex
                    };
                    
                    characters[character.id] = characterData;
                    
                    Debug.Log($"[DummyCharacterProviderApi] Cargado personaje: {character.charactername} id={character.id} nivel={character.characterlevel} exp={character.characterexperience}");
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
            // IMPORTANTE: Mezclar los personajes en memoria con los existentes en el archivo
            // para no perder personajes de otros usuarios al guardar
            var existingJson = LoadCharactersJson();
            var existingCharacters = new Dictionary<int, CharacterData>();
            
            // Cargar personajes existentes del archivo
            if (!string.IsNullOrEmpty(existingJson))
            {
                var existingCollection = JsonUtility.FromJson<CharacterDataCollection>(existingJson);
                if (existingCollection?.items != null)
                {
                    foreach (var character in existingCollection.items)
                    {
                        existingCharacters[character.id] = character;
                    }
                }
            }
            
            // Actualizar/agregar los personajes en memoria (sobrescribe los existentes con mismo ID)
            foreach (var kvp in characters)
            {
                existingCharacters[kvp.Key] = kvp.Value;
                Debug.Log($"[DummyCharacterProviderApi] Guardando: {kvp.Value.charactername} id={kvp.Key} nivel={kvp.Value.characterlevel} exp={kvp.Value.characterexperience}");
            }
            
            // Guardar la colección combinada
            var mergedCollection = new CharacterDataCollection(existingCharacters.Values.ToArray());
            var json = mergedCollection.ToString();
            
            Debug.Log($"[DummyCharacterProviderApi] SaveCharacterCollection: {json}");

            // Usar ISaveService si está disponible
            if (saveService != null)
            {
                saveService.SetString(StorageKey, json);
                saveService.Save();
                Debug.Log("[DummyCharacterProviderApi] Guardado con ISaveService");
            }
            else
            {
                // Fallback a PlayerPrefs
                PlayerPrefs.DeleteKey(StorageKey);
                PlayerPrefs.SetString(StorageKey, json);
                PlayerPrefs.Save();
                Debug.Log("[DummyCharacterProviderApi] Guardado con PlayerPrefs (fallback)");
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