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
    /// </summary>
    public class PlayerRepository : IPlayerRepository
    {
        private readonly ISaveService saveService;
        private Dictionary<string, PlayerCredentialsData> players;

        /// <summary>
        /// Constructor que recibe el servicio de persistencia.
        /// </summary>
        /// <param name="saveService">Servicio de guardado.</param>
        public PlayerRepository(ISaveService saveService)
        {
            this.saveService = saveService;
            this.players = new Dictionary<string, PlayerCredentialsData>();
            LoadPlayers();
        }

        /// <inheritdoc/>
        public bool PlayerExists(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return false;
            }
            return players.ContainsKey(playerName.ToLowerInvariant());
        }

        /// <inheritdoc/>
        public IPlayerCredentials GetPlayer(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return null;
            }

            var key = playerName.ToLowerInvariant();
            return players.TryGetValue(key, out var player) ? player : null;
        }

        /// <inheritdoc/>
        public bool RegisterPlayer(string playerName, string password)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            var key = playerName.ToLowerInvariant();
            if (players.ContainsKey(key))
            {
                return false;
            }

            var newPlayer = new PlayerCredentialsData(playerName, password);
            players.Add(key, newPlayer);
            SavePlayers();

            Debug.Log($"[PlayerRepository] Jugador registrado: {playerName}");
            return true;
        }

        /// <inheritdoc/>
        public bool UpdatePassword(string playerName, string newPassword)
        {
            if (string.IsNullOrEmpty(playerName) || string.IsNullOrEmpty(newPassword))
            {
                return false;
            }

            var key = playerName.ToLowerInvariant();
            if (!players.TryGetValue(key, out var player))
            {
                return false;
            }

            player.Password = newPassword;
            SavePlayers();

            Debug.Log($"[PlayerRepository] Contraseña actualizada para: {playerName}");
            return true;
        }

        /// <inheritdoc/>
        public int IncrementFailedAttempts(string playerName)
        {
            if (string.IsNullOrEmpty(playerName))
            {
                return 0;
            }

            var key = playerName.ToLowerInvariant();
            if (!players.TryGetValue(key, out var player))
            {
                return 0;
            }

            player.FailedAttempts++;
            
            // Auto-bloquear si excede el límite
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

            var key = playerName.ToLowerInvariant();
            if (players.TryGetValue(key, out var player))
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

            var key = playerName.ToLowerInvariant();
            if (players.TryGetValue(key, out var player))
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

            var key = playerName.ToLowerInvariant();
            if (players.TryGetValue(key, out var player))
            {
                player.IsBlocked = false;
                player.FailedAttempts = 0;
                SavePlayers();
                Debug.Log($"[PlayerRepository] Jugador desbloqueado: {playerName}");
            }
        }

        /// <inheritdoc/>
        public void UnblockAllPlayers()
        {
            foreach (var player in players.Values)
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
            return players.Values
                .Where(p => p.IsBlocked)
                .Select(p => p.PlayerName)
                .ToList()
                .AsReadOnly();
        }

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
                players = new Dictionary<string, PlayerCredentialsData>();
                return;
            }

            var collection = PlayerCredentialsCollection.FromJson(json);
            players = new Dictionary<string, PlayerCredentialsData>();

            if (collection?.players != null)
            {
                foreach (var player in collection.players)
                {
                    if (!string.IsNullOrEmpty(player.PlayerName))
                    {
                        players[player.PlayerName.ToLowerInvariant()] = player;
                    }
                }
            }

            Debug.Log($"[PlayerRepository] Cargados {players.Count} jugadores");
        }

        private void SavePlayers()
        {
            if (saveService == null)
            {
                Debug.LogWarning("[PlayerRepository] SaveService no disponible, no se pueden guardar datos");
                return;
            }

            var collection = new PlayerCredentialsCollection(players.Values.ToArray());
            var json = collection.ToJson();

            saveService.SetString(PlayerLoginSettings.PlayersStorageKey, json);
            saveService.Save();
        }
    }
}
