using System.Collections.Generic;
using Scripts.Core.Domain.Interfaces;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de entidades.
    /// Almacena y gestiona todas las entidades del juego.
    /// </summary>
    public class EntityRepository : IEntityRepository
    {
        private readonly Dictionary<int, IGameEntity> entities;
        private IGameEntity localEntity;

        public EntityRepository()
        {
            entities = new Dictionary<int, IGameEntity>();
        }

        public int Count => entities.Count;

        public IGameEntity GetLocalEntity()
        {
            return localEntity;
        }

        public void SetLocalEntity(IGameEntity entity)
        {
            localEntity = entity;
            
            if (entity != null)
            {
                Debug.Log($"[EntityRepository] Local entity set with id #{entity.Id}");
            }
        }

        public bool TryGetEntity(int id, out IGameEntity entity)
        {
            return entities.TryGetValue(id, out entity);
        }

        public bool ContainsEntity(int id)
        {
            return entities.ContainsKey(id);
        }

        public void AddEntity(int id, IGameEntity entity)
        {
            if (entity == null)
            {
                Debug.LogWarning($"[EntityRepository] Cannot add null entity with id #{id}");
                return;
            }

            if (entities.ContainsKey(id))
            {
                Debug.LogWarning($"[EntityRepository] Entity with id #{id} already exists. Replacing.");
                entities[id] = entity;
            }
            else
            {
                entities.Add(id, entity);
                Debug.Log($"[EntityRepository] Added entity with id #{id}. Total: {entities.Count}");
            }
        }

        public bool RemoveEntity(int id)
        {
            if (entities.Remove(id))
            {
                Debug.Log($"[EntityRepository] Removed entity with id #{id}. Remaining: {entities.Count}");
                
                // Si la entidad removida era la local, limpiarla
                if (localEntity != null && localEntity.Id == id)
                {
                    localEntity = null;
                }
                
                return true;
            }

            Debug.LogWarning($"[EntityRepository] Entity with id #{id} not found for removal");
            return false;
        }

        public void Clear()
        {
            var count = entities.Count;
            entities.Clear();
            localEntity = null;
            Debug.Log($"[EntityRepository] Cleared {count} entities");
        }
    }
}
