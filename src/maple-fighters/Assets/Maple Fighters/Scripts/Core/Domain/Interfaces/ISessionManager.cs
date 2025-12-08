using System;
using Scripts.Core.Infrastructure;
using UnityEngine;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para gestión de sesiones.
    /// </summary>
    public interface ISessionManager
    {
        /// <summary>
        /// Datos de la sesión actual.
        /// </summary>
        SessionData CurrentSession { get; }

        /// <summary>
        /// Indica si hay una sesión activa válida.
        /// </summary>
        bool HasValidSession { get; }

        /// <summary>
        /// Crea una nueva sesión.
        /// </summary>
        void CreateSession(string email, string playerName, string characterClass = null);

        /// <summary>
        /// Carga la sesión guardada (si existe y es válida).
        /// </summary>
        /// <returns>True si se cargó una sesión válida.</returns>
        bool LoadSession();

        /// <summary>
        /// Guarda la sesión actual.
        /// </summary>
        void SaveSession();

        /// <summary>
        /// Cierra la sesión actual (logout).
        /// </summary>
        void ClearSession();

        /// <summary>
        /// Renueva el token de la sesión actual.
        /// </summary>
        void RefreshSession();

        /// <summary>
        /// Evento disparado cuando la sesión cambia.
        /// </summary>
        event Action<SessionData> SessionChanged;
    }
}
