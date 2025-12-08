using System;
using System.Collections.Generic;
using System.Linq;
using Scripts.Core.Domain.Interfaces;
using Scripts.Services.PlayerLoginApi;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Repositories
{
    /// <summary>
    /// Implementación del repositorio de jugadores.
    /// Usa ISaveService para persistencia de datos.
    /// Soporta tanto búsqueda por email (v2) como por nombre (v1 legacy).
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ISaveService saveService;
        
        // Índice principal por email (clave primaria en v2)
        private Dictionary<string, PlayerCredentialsData> playersByEmail;
        
        // Índice secundario por nombre (para compatibilidad con v1)
        private Dictionary<string, PlayerCredentialsData> playersByName;

        /// <summary>
        /// Constructor que recibe el servicio de persistencia.
        /// </summary>
        /// <param name="saveService">Servicio de guardado.</param>
        public PlayerRepository(ISaveService saveService)
        {
            this.saveService = saveService;
            this.playersByEmail = new Dictionary<string, PlayerCredentialsData>(StringComparer.OrdinalIgnoreCase);
            this.playersByName = new Dictionary<string, PlayerCredentialsData>(StringComparer.OrdinalIgnoreCase);
            LoadPlayers();
        }

        #region Email-based Methods (v2)

        /// <inheritdoc/>
        public bool EmailExists(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return false;
            }
            return playersByEmail.ContainsKey(email);
        }

        /// <inheritdoc/>
        public IPlayerCredentials GetPlayerByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }
            return playersByEmail.TryGetValue(email, out var player) ? player : null;
        }

        /// <inheritdoc/>
        public IPlayerCredentials CreatePlayerWithEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return null;
            }

            if (playersByEmail.ContainsKey(email))
            {
                Debug.LogWarning($"[PlayerRepository] Email ya existe: {email}");
                return null;
            }

            var newPlayer = new PlayerCredentialsData(email);
            playersByEmail[email] = newPlayer;
            SavePlayers();

            Debug.Log($"[PlayerRepository] Jugador creado con email: {email}");
            return newPlayer;
        }

        /// <inheritdoc/>
        public IPlayerCredentials CreatePlayerWithEmailAndPassword(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return null;
            }

            if (playersByEmail.ContainsKey(email))
            {
                Debug.LogWarning($"[PlayerRepository] Email ya existe: {email}");
                return null;
            }

            var newPlayer = new PlayerCredentialsData(email);
            newPlayer.Password = password;
            playersByEmail[email] = newPlayer;
            SavePlayers();

            Debug.Log($"[PlayerRepository] Jugador creado con email y contraseña: {email}");
            return newPlayer;
        }

        /// <inheritdoc/>
        public bool CompleteRegistration(string email, string playerName, string password, string characterClass)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(playerName) || 
                string.IsNullOrEmpty(password) || string.IsNullOrEmpty(characterClass))
            {
                return false;
            }

            if (!playersByEmail.TryGetValue(email, out var player))
            {
                Debug.LogWarning($"[PlayerRepository] Email no encontrado para completar registro: {email}");
                return false;
            }

            // Verificar que el nombre no esté en uso
            var nameKey = playerName.ToLowerInvariant();
            if (playersByName.ContainsKey(nameKey))
            {
                Debug.LogWarning($"[PlayerRepository] Nombre de jugador ya en uso: {playerName}");
                return false;
            }

            // Completar los datos
            player.PlayerName = playerName;
            player.Password = password;
            player.CharacterClass = characterClass;
            player.CharacterId = Guid.NewGuid().ToString();

            // Agregar al índice por nombre
            playersByName[nameKey] = player;

            SavePlayers();

            Debug.Log($"[PlayerRepository] Registro completado para: {email} -> {playerName} ({characterClass})");
            return true;
        }

        /// <inheritdoc/>
        public void UpdateLastLogin(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            if (playersByEmail.TryGetValue(email, out var player))
            {
                player.UpdateLastLogin();
                SavePlayers();
            }
        }

        /// <inheritdoc/>
        public int IncrementFailedAttemptsByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return 0;
            }

            if (!playersByEmail.TryGetValue(email, out var player))
            {
                return 0;
            }

            player.FailedAttempts++;

            if (player.FailedAttempts >= PlayerLoginSettings.MaxLoginAttempts)
            {
                player.IsBlocked = true;
                Debug.Log($"[PlayerRepository] Jugador bloqueado por intentos fallidos: {email}");
            }

            SavePlayers();
            return player.FailedAttempts;
        }

        /// <inheritdoc/>
        public void ResetFailedAttemptsByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            if (playersByEmail.TryGetValue(email, out var player))
            {
                player.FailedAttempts = 0;
                SavePlayers();
            }
        }

        /// <inheritdoc/>
        public void BlockPlayerByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            if (playersByEmail.TryGetValue(email, out var player))
            {
                player.IsBlocked = true;
                SavePlayers();
                Debug.Log($"[PlayerRepository] Jugador bloqueado: {email}");
            }
        }

        /// <inheritdoc/>
        public void UnblockPlayerByEmail(string email)
        {
            if (string.IsNullOrEmpty(email))
            {
                return;
            }

            if (playersByEmail.TryGetValue(email, out var player))
            {
                player.IsBlocked = false;
                player.FailedAttempts = 0;
                SavePlayers();
                Debug.Log($"[PlayerRepository] Jugador desbloqueado: {email}");
            }
        }

        #endregion

        #region PlayerName-based Methods (legacy v1)

        /// <inheritdoc/>
        public bool PlayerExists(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return false;
            }
            return playersByName.ContainsKey(playerName);
        }

        /// <inheritdoc/>
        public IPlayerCredentials GetPlayer(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return null;
            }
            return playersByName.TryGetValue(playerName, out var player) ? player : null;
        }

        /// <inheritdoc/>
        public bool RegisterPlayer(string playerName, string password)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            if (playersByName.ContainsKey(playerName))
            {
                return false;
            }

            #pragma warning disable CS0618 // Type or member is obsolete
            var newPlayer = new PlayerCredentialsData(playerName, password);
            #pragma warning restore CS0618

            // Agregar a ambos índices
            playersByEmail[newPlayer.Email] = newPlayer;
            playersByName[playerName] = newPlayer;

            SavePlayers();

            Debug.Log($"[PlayerRepository] Jugador registrado (legacy): {playerName}");
            return true;
        }

        /// <inheritdoc/>
        public bool UpdatePassword(string playerName, string newPassword)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(newPassword))
            {
                return false;
            }

            if (!playersByName.TryGetValue(playerName, out var player))
            {
                return false;
            }

            player.Password = newPassword;
            SavePlayers();

            Debug.Log($"[PlayerRepository] Contraseña actualizada para: {playerName}");
            return true;
        }

        /// <inheritdoc/>
        public bool UpdatePasswordByEmail(string email, string newPassword)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(newPassword))
            {
                return false;
            }

            if (!playersByEmail.TryGetValue(email, out var player))
            {
                Debug.LogWarning($"[PlayerRepository] No se encontró jugador con email: {email}");
                return false;
            }

            player.Password = newPassword;
            SavePlayers();

            Debug.Log($"[PlayerRepository] Contraseña actualizada por email para: {email}");
            return true;
        }

        /// <inheritdoc/>
        public int IncrementFailedAttempts(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return 0;
            }

            if (!playersByName.TryGetValue(playerName, out var player))
            {
                return 0;
            }

            player.FailedAttempts++;

            if (player.FailedAttempts >= PlayerLoginSettings.MaxLoginAttempts)
            {
                player.IsBlocked = true;
                Debug.Log($"[PlayerRepository] Jugador bloqueado por intentos fallidos: {playerName}");
            }

            SavePlayers();
            return player.FailedAttempts;
        }

        /// <inheritdoc/>
        public void ResetFailedAttempts(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            if (playersByName.TryGetValue(playerName, out var player))
            {
                player.FailedAttempts = 0;
                SavePlayers();
            }
        }

        /// <inheritdoc/>
        public void BlockPlayer(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            if (playersByName.TryGetValue(playerName, out var player))
            {
                player.IsBlocked = true;
                SavePlayers();
                Debug.Log($"[PlayerRepository] Jugador bloqueado: {playerName}");
            }
        }

        /// <inheritdoc/>
        public void UnblockPlayer(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return;
            }

            if (playersByName.TryGetValue(playerName, out var player))
            {
                player.IsBlocked = false;
                player.FailedAttempts = 0;
                SavePlayers();
                Debug.Log($"[PlayerRepository] Jugador desbloqueado: {playerName}");
            }
        }

        #endregion

        #region Common Methods

        /// <inheritdoc/>
        public void UnblockAllPlayers()
        {
            foreach (var player in playersByEmail.Values)
            {
                player.IsBlocked = false;
                player.FailedAttempts = 0;
            }
            SavePlayers();
            Debug.Log("[PlayerRepository] Todos los jugadores desbloqueados");
        }

        /// <inheritdoc/>
        public IReadOnlyList<string> GetBlockedPlayers()
        {
            return playersByEmail.Values
                .Where(p => p.IsBlocked)
                .Select(p => string.IsNullOrEmpty(p.PlayerName) ? p.Email : p.PlayerName)
                .ToList()
                .AsReadOnly();
        }

        #endregion

        #region Persistence

        private void LoadPlayers()
        {
            if (saveService == null)
            {
                Debug.LogWarning("[PlayerRepository] SaveService no disponible, usando memoria local");
                return;
            }

            var json = saveService.GetString(PlayerLoginSettings.PlayersStorageKey, string.Empty);
            if (string.IsNullOrEmpty(json))
            {
                playersByEmail = new Dictionary<string, PlayerCredentialsData>(StringComparer.OrdinalIgnoreCase);
                playersByName = new Dictionary<string, PlayerCredentialsData>(StringComparer.OrdinalIgnoreCase);
                return;
            }

            var collection = PlayerCredentialsCollection.FromJson(json);
            playersByEmail = new Dictionary<string, PlayerCredentialsData>(StringComparer.OrdinalIgnoreCase);
            playersByName = new Dictionary<string, PlayerCredentialsData>(StringComparer.OrdinalIgnoreCase);

            if (collection?.players != null)
            {
                foreach (var player in collection.players)
                {
                    // Índice por email (siempre disponible en v2)
                    if (!string.IsNullOrEmpty(player.Email))
                    {
                        playersByEmail[player.Email] = player;
                    }

                    // Índice por nombre (si está disponible)
                    if (!string.IsNullOrEmpty(player.PlayerName))
                    {
                        playersByName[player.PlayerName] = player;
                    }
                }
            }

            Debug.Log($"[PlayerRepository] Cargados {playersByEmail.Count} jugadores (por email), {playersByName.Count} (por nombre)");
        }

        private void SavePlayers()
        {
            if (saveService == null)
            {
                Debug.LogWarning("[PlayerRepository] SaveService no disponible, no se pueden guardar datos");
                return;
            }

            // Guardar todos los jugadores únicos (usando el índice por email como fuente principal)
            var collection = new PlayerCredentialsCollection(playersByEmail.Values.ToArray());
            var json = collection.ToJson();

            saveService.SetString(PlayerLoginSettings.PlayersStorageKey, json);
            saveService.Save();
        }

        #endregion
    }
}
