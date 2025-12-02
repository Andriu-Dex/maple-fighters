using System;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interfaz para la sesión del usuario.
    /// Contiene información del usuario logueado y su personaje actual.
    /// </summary>
    public interface IUserSession
    {
        /// <summary>
        /// Evento cuando se agregan puntos de experiencia al personaje.
        /// </summary>
        event Action<float> CharacterExperiencePointsAdded;

        /// <summary>
        /// Evento cuando el personaje sube de nivel.
        /// </summary>
        event Action<int> CharacterLevelUp;

        /// <summary>
        /// ID único del usuario.
        /// </summary>
        string UserId { get; }

        /// <summary>
        /// ID del personaje actual.
        /// </summary>
        int CharacterId { get; set; }

        /// <summary>
        /// Tipo/clase del personaje.
        /// </summary>
        int CharacterType { get; set; }

        /// <summary>
        /// Salud actual del personaje.
        /// </summary>
        int CharacterHealth { get; set; }

        /// <summary>
        /// Nivel del personaje.
        /// </summary>
        int CharacterLevel { get; set; }

        /// <summary>
        /// Nombre del personaje.
        /// </summary>
        string CharacterName { get; set; }

        /// <summary>
        /// Puntos de experiencia actuales.
        /// </summary>
        float CharacterExperiencePoints { get; set; }

        /// <summary>
        /// Indica si el usuario está logueado.
        /// </summary>
        bool IsLoggedIn { get; set; }

        /// <summary>
        /// Obtiene la salud máxima del personaje.
        /// </summary>
        int GetMaxCharacterHealth();

        /// <summary>
        /// Obtiene los puntos de experiencia actuales.
        /// </summary>
        float GetExperiencePoints();

        /// <summary>
        /// Obtiene los puntos de experiencia máximos para el nivel actual.
        /// </summary>
        float GetMaxExperiencePoints();

        /// <summary>
        /// Agrega puntos de experiencia al personaje.
        /// </summary>
        void AddExperiencePoints(float value);
    }
}
