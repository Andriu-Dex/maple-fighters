using UnityEngine;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interfaz para el repositorio de entidades del juego.
    /// Permite gestionar entidades (jugador local, jugadores remotos) de forma desacoplada.
    /// </summary>
    public interface IEntityRepository
    {
        /// <summary>
        /// Obtiene la entidad del jugador local.
        /// </summary>
        IGameEntity GetLocalEntity();

        /// <summary>
        /// Intenta obtener una entidad remota por su ID.
        /// </summary>
        /// <param name="id">ID de la entidad</param>
        /// <param name="entity">Entidad encontrada</param>
        /// <returns>True si la entidad existe</returns>
        bool TryGetEntity(int id, out IGameEntity entity);

        /// <summary>
        /// Verifica si existe una entidad con el ID dado.
        /// </summary>
        bool ContainsEntity(int id);

        /// <summary>
        /// Agrega una entidad al repositorio.
        /// </summary>
        /// <param name="id">ID único de la entidad</param>
        /// <param name="entity">Entidad a agregar</param>
        void AddEntity(int id, IGameEntity entity);

        /// <summary>
        /// Remueve una entidad del repositorio.
        /// </summary>
        /// <param name="id">ID de la entidad a remover</param>
        /// <returns>True si se removió exitosamente</returns>
        bool RemoveEntity(int id);

        /// <summary>
        /// Establece la entidad del jugador local.
        /// </summary>
        void SetLocalEntity(IGameEntity entity);

        /// <summary>
        /// Obtiene el número de entidades en el repositorio.
        /// </summary>
        int Count { get; }

        /// <summary>
        /// Limpia todas las entidades del repositorio.
        /// </summary>
        void Clear();
    }

    /// <summary>
    /// Interfaz base para entidades del juego.
    /// Abstrae la dependencia de Scripts.Gameplay.Entity.IEntity
    /// </summary>
    public interface IGameEntity
    {
        int Id { get; set; }
        GameObject GameObject { get; }
    }
}
