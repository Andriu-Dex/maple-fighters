using System.Collections;
using System.Collections.Generic;
using Game.Messages;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Gameplay.Graphics;
using Scripts.Gameplay.Player;
using Scripts.Services;
using Scripts.Services.GameApi;
using UnityEngine;

namespace Scripts.Gameplay.Entity
{
    /// <summary>
    /// Contenedor de entidades del juego.
    /// Refactorizado para usar IEntityRepository e IEntityFactory del ServiceLocator.
    /// Mantiene compatibilidad hacia atrás si los servicios no están disponibles.
    /// </summary>
    public class EntityContainer : MonoBehaviour
    {
        public static EntityContainer GetInstance()
        {
            if (instance == null)
            {
                instance = FindObjectOfType<EntityContainer>();
            }

            return instance;
        }

        private static EntityContainer instance;

        private IGameApi gameApi;
        
        // Nuevos servicios inyectados
        private IEntityRepository entityRepository;
        private IEntityFactory entityFactory;
        
        // Fallback: colección local si el repositorio no está disponible
        private Dictionary<int, IEntity> localCollection;
        private IEntity localEntity;

        private void Awake()
        {
            // Intentar obtener servicios del ServiceLocator
            ServiceLocator.TryGet(out entityRepository);
            ServiceLocator.TryGet(out entityFactory);
            
            // Fallback si no hay repositorio
            if (entityRepository == null)
            {
                localCollection = new Dictionary<int, IEntity>();
                Debug.Log("[EntityContainer] Using local collection (fallback mode)");
            }
            else
            {
                Debug.Log("[EntityContainer] Using IEntityRepository from ServiceLocator");
            }
        }

        private void Start()
        {
            gameApi = ApiProvider.ProvideGameApi();
            gameApi.SceneEntered.AddListener(OnSceneEntered);
            gameApi.GameObjectsAdded.AddListener(OnGameObjectsAdded);
            gameApi.GameObjectsRemoved.AddListener(OnGameObjectsRemoved);
        }

        private void OnDisable()
        {
            gameApi?.SceneEntered?.RemoveListener(OnSceneEntered);
            gameApi?.GameObjectsAdded?.RemoveListener(OnGameObjectsAdded);
            gameApi?.GameObjectsRemoved?.RemoveListener(OnGameObjectsRemoved);
            
            // Limpiar el repositorio al cambiar de escena para evitar referencias a objetos destruidos
            entityRepository?.Clear();
            localCollection?.Clear();
            localEntity = null;
            
            // Limpiar la instancia singleton
            if (instance == this)
            {
                instance = null;
            }
        }

        private void OnSceneEntered(EnteredSceneMessage message)
        {
            var name = "LocalPlayer";
            var id = message.GameObjectId;
            var x = message.X;
            var y = message.Y;
            var position = new Vector2(x, y);
            var direction = message.Direction;

            var entity = AddEntity(id, name, position);
            
            // Guardar referencia local y en repositorio
            localEntity = entity;
            entityRepository?.SetLocalEntity(entity);

            if (entity != null && direction != 0)
            {
                var entityGameObject = entity.GameObject;
                StartCoroutine(SetEntityDirection(entityGameObject, direction));
            }
        }

        private void OnGameObjectsAdded(GameObjectsAddedMessage message)
        {
            var gameObjects = message.GameObjects;

            StartCoroutine(AddEntities(gameObjects));
        }

        private IEnumerator AddEntities(GameObjectData[] gameObjects)
        {
            foreach (var gameObject in gameObjects)
            {
                var id = gameObject.Id;
                var name = gameObject.Name;
                var position = new Vector2(gameObject.X, gameObject.Y);
                var direction = gameObject.Direction;

                if (ContainsEntity(id))
                {
                    Debug.LogWarning($"The entity with id #{id} already exists.");
                }
                else
                {
                    var entity = AddEntity(id, name, position);

                    if (entity != null && direction != 0)
                    {
                        var entityGameObject = entity.GameObject;
                        StartCoroutine(SetEntityDirection(entityGameObject, direction));
                    }
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private void OnGameObjectsRemoved(GameObjectsRemovedMessage message)
        {
            var gameObjectIds = message.GameObjectIds;

            StartCoroutine(RemoveEntities(gameObjectIds));
        }

        private IEnumerator RemoveEntities(int[] gameObjectIds)
        {
            foreach (var id in gameObjectIds)
            {
                if (TryGetEntityInternal(id, out var entity))
                {
                    RemoveEntity(entity);
                }

                yield return new WaitForEndOfFrame();
            }
        }

        private IEntity AddEntity(int id, string name, Vector2 position)
        {
            IEntity entity = null;

            // Usar factory si está disponible, sino el método original
            if (entityFactory != null)
            {
                var gameEntity = entityFactory.CreateEntity(name, position);
                entity = gameEntity as IEntity;
            }
            else
            {
                var gameObject = Utils.CreateGameObject(name, position);
                if (gameObject != null)
                {
                    entity = gameObject.GetComponent<IEntity>();
                }
            }

            if (entity != null)
            {
                entity.Id = id;

                // Agregar al repositorio o colección local
                if (entityRepository != null)
                {
                    entityRepository.AddEntity(id, entity);
                }
                else
                {
                    localCollection.Add(id, entity);
                }

                Debug.Log($"Added a new entity with id #{id}");
            }

            return entity;
        }

        private void RemoveEntity(IEntity entity)
        {
            var id = entity.Id;

            // Usar factory para destruir si está disponible
            if (entityFactory != null)
            {
                entityFactory.DestroyEntity(entity);
            }
            else
            {
                // Fallback: destrucción manual
                var gameObject = entity.GameObject;
                var fadeEffectProvider = gameObject.GetComponent<IFadeEffectProvider>();
                
                if (fadeEffectProvider != null)
                {
                    var fadeEffect = fadeEffectProvider.Provide();
                    if (fadeEffect != null)
                    {
                        fadeEffect.UnFadeAndDestroyGameObject();
                    }
                    else
                    {
                        Destroy(gameObject);
                    }
                }
                else
                {
                    Destroy(gameObject);
                }
            }

            // Remover del repositorio o colección local
            if (entityRepository != null)
            {
                entityRepository.RemoveEntity(id);
            }
            else
            {
                localCollection.Remove(id);
            }

            Debug.Log($"Removed an entity with id #{id}");
        }

        /// <summary>
        /// Verifica si existe una entidad con el ID dado.
        /// </summary>
        private bool ContainsEntity(int id)
        {
            if (entityRepository != null)
            {
                return entityRepository.ContainsEntity(id);
            }
            return localCollection.ContainsKey(id);
        }

        /// <summary>
        /// Intenta obtener una entidad por ID (uso interno).
        /// </summary>
        private bool TryGetEntityInternal(int id, out IEntity entity)
        {
            if (entityRepository != null)
            {
                var found = entityRepository.TryGetEntity(id, out var gameEntity);
                entity = gameEntity as IEntity;
                return found;
            }
            return localCollection.TryGetValue(id, out entity);
        }

        public IEntity GetLocalEntity()
        {
            // Preferir el repositorio si está disponible
            if (entityRepository != null)
            {
                return entityRepository.GetLocalEntity() as IEntity;
            }
            return localEntity;
        }

        public bool GetRemoteEntity(int id, out IEntity entity)
        {
            return TryGetEntityInternal(id, out entity);
        }

        private IEnumerator SetEntityDirection(GameObject entity, float direction)
        {
            yield return new WaitForSeconds(0.1f);

            var name = entity.name;
            if (name == "LocalPlayer" || name == "RemotePlayer")
            {
                var spawnedCharacter =
                    entity.GetComponent<ISpawnedCharacter>();
                var character = spawnedCharacter?.GetCharacter();
                if (character != null)
                {
                    var playerController =
                        character.GetComponent<PlayerController>();
                    if (playerController != null)
                    {
                        playerController.SetDirection(direction);
                    }
                }
            }
            else
            {
                var x = direction;
                var y = entity.transform.localScale.y;
                var z = entity.transform.localScale.z;

                entity.transform.localScale = new Vector3(x, y, z);
            }
        }
    }
}