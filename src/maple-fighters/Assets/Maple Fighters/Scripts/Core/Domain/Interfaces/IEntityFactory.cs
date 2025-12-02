using UnityEngine;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interfaz para la fábrica de entidades del juego.
    /// Encapsula la lógica de creación de entidades.
    /// </summary>
    public interface IEntityFactory
    {
        /// <summary>
        /// Crea una entidad en la posición especificada.
        /// </summary>
        /// <param name="name">Nombre/tipo de la entidad (LocalPlayer, RemotePlayer, etc.)</param>
        /// <param name="position">Posición inicial</param>
        /// <returns>La entidad creada o null si falla</returns>
        IGameEntity CreateEntity(string name, Vector2 position);

        /// <summary>
        /// Destruye una entidad aplicando efectos de fade si están disponibles.
        /// </summary>
        /// <param name="entity">Entidad a destruir</param>
        void DestroyEntity(IGameEntity entity);
    }
}
