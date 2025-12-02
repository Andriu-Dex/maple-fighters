using Scripts.Core.Domain.Interfaces;
using UnityEngine;

namespace Scripts.Gameplay.Entity
{
    /// <summary>
    /// Interfaz para entidades del juego.
    /// Hereda de IGameEntity para compatibilidad con el sistema de repositorio.
    /// </summary>
    public interface IEntity : IGameEntity
    {
        // Id y GameObject heredados de IGameEntity
    }
}