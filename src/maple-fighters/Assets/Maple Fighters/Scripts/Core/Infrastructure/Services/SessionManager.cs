using System;
using Scripts.Core.Domain.Interfaces;
using UnityEngine;

namespace Scripts.Core.Infrastructure
{
    /// <summary>
    /// Implementación del gestor de sesiones usando ISaveService.
    /// </summary>
    public class SessionManager : ISessionManager
    {
        private const string SessionKey = "player_session";
        
        private readonly ISaveService saveService;
        private SessionData currentSession;

        public SessionData CurrentSession => currentSession;
        public bool HasValidSession => currentSession != null && currentSession.IsValid();

        public event Action<SessionData> SessionChanged;

        public SessionManager(ISaveService saveService)
        {
            this.saveService = saveService;
        }

        public void CreateSession(string email, string playerName, string characterClass = null)
        {
            currentSession = new SessionData(email, playerName, characterClass);
            SaveSession();
            
            Debug.Log($"[SessionManager] Sesión creada: {currentSession}");
            SessionChanged?.Invoke(currentSession);
        }

        public bool LoadSession()
        {
            if (saveService == null)
            {
                Debug.LogWarning("[SessionManager] SaveService no disponible");
                return false;
            }

            var json = saveService.GetString(SessionKey, "");
            
            if (string.IsNullOrEmpty(json))
            {
                Debug.Log("[SessionManager] No hay sesión guardada");
                return false;
            }

            try
            {
                currentSession = JsonUtility.FromJson<SessionData>(json);
                
                if (currentSession == null || !currentSession.IsValid())
                {
                    Debug.Log("[SessionManager] Sesión expirada o inválida");
                    ClearSession();
                    return false;
                }

                Debug.Log($"[SessionManager] Sesión cargada: {currentSession}");
                SessionChanged?.Invoke(currentSession);
                return true;
            }
            catch (Exception e)
            {
                Debug.LogError($"[SessionManager] Error al cargar sesión: {e.Message}");
                ClearSession();
                return false;
            }
        }

        public void SaveSession()
        {
            if (saveService == null || currentSession == null)
            {
                return;
            }

            var json = JsonUtility.ToJson(currentSession);
            saveService.SetString(SessionKey, json);
            saveService.Save();
            
            Debug.Log("[SessionManager] Sesión guardada");
        }

        public void ClearSession()
        {
            currentSession = null;
            
            if (saveService != null)
            {
                saveService.SetString(SessionKey, "");
                saveService.Save();
            }

            Debug.Log("[SessionManager] Sesión eliminada");
            SessionChanged?.Invoke(null);
        }

        public void RefreshSession()
        {
            if (currentSession != null)
            {
                currentSession.Refresh();
                SaveSession();
                Debug.Log($"[SessionManager] Sesión renovada: {currentSession}");
            }
        }
    }
}
