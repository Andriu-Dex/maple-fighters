using Scripts.Constants;
using Scripts.Core.Domain.Interfaces;
using Scripts.Gameplay.Entity;
using Scripts.Gameplay.Graphics;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Factories
{
    /// <summary>
    /// Implementación de la fábrica de entidades.
    /// Crea y destruye entidades del juego.
    /// </summary>
    public class EntityFactory : IEntityFactory
    {
        public IGameEntity CreateEntity(string name, Vector2 position)
        {
            var path = string.Format(Paths.Resources.Game.Entities, name);
            var prefab = Resources.Load(path);
            
            if (prefab == null)
            {
                Debug.LogError($"[EntityFactory] Could not find entity prefab at: {path}");
                return null;
            }

            var gameObject = Object.Instantiate(prefab, position, Quaternion.identity) as GameObject;
            
            if (gameObject == null)
            {
                Debug.LogError($"[EntityFactory] Failed to instantiate entity: {name}");
                return null;
            }

            gameObject.name = name;

            var entity = gameObject.GetComponent<IEntity>();
            
            if (entity == null)
            {
                Debug.LogError($"[EntityFactory] Entity prefab {name} does not have IEntity component");
                Object.Destroy(gameObject);
                return null;
            }

            Debug.Log($"[EntityFactory] Created entity: {name} at {position}");
            return entity;
        }

        public void DestroyEntity(IGameEntity entity)
        {
            if (entity == null)
            {
                Debug.LogWarning("[EntityFactory] Cannot destroy null entity");
                return;
            }

            var gameObject = entity.GameObject;
            
            if (gameObject == null)
            {
                Debug.LogWarning($"[EntityFactory] Entity {entity.Id} has null GameObject");
                return;
            }

            // Intentar usar el efecto de fade si está disponible
            var fadeEffectProvider = gameObject.GetComponent<IFadeEffectProvider>();
            
            if (fadeEffectProvider != null)
            {
                var fadeEffect = fadeEffectProvider.Provide();
                
                if (fadeEffect != null)
                {
                    fadeEffect.UnFadeAndDestroyGameObject();
                    Debug.Log($"[EntityFactory] Destroying entity {entity.Id} with fade effect");
                    return;
                }
            }

            // Destruir directamente si no hay fade
            Object.Destroy(gameObject);
            Debug.Log($"[EntityFactory] Destroyed entity {entity.Id}");
        }
    }
}
