using UnityEditor;
using UnityEngine;

namespace Scripts.Editor
{
    /// <summary>
    /// Editor utility to clean player data for testing purposes.
    /// </summary>
    public static class PlayerDataCleaner
    {
        private const string PlayersStorageKey = "player_credentials";
        private const string SessionStorageKey = "maple_fighters_session";

        [MenuItem("Tools/Maple Fighters/Clear Player Data")]
        public static void ClearPlayerData()
        {
            if (EditorUtility.DisplayDialog(
                "Clear Player Data",
                "This will delete all saved player accounts and session data. Are you sure?",
                "Yes, Clear All",
                "Cancel"))
            {
                PlayerPrefs.DeleteKey(PlayersStorageKey);
                PlayerPrefs.DeleteKey(SessionStorageKey);
                PlayerPrefs.Save();
                
                Debug.Log("[PlayerDataCleaner] Player data cleared successfully!");
                EditorUtility.DisplayDialog("Success", "Player data has been cleared.", "OK");
            }
        }

        [MenuItem("Tools/Maple Fighters/Clear All PlayerPrefs")]
        public static void ClearAllPlayerPrefs()
        {
            if (EditorUtility.DisplayDialog(
                "Clear All PlayerPrefs",
                "WARNING: This will delete ALL PlayerPrefs data for this project. Are you sure?",
                "Yes, Clear Everything",
                "Cancel"))
            {
                PlayerPrefs.DeleteAll();
                PlayerPrefs.Save();
                
                Debug.Log("[PlayerDataCleaner] All PlayerPrefs cleared!");
                EditorUtility.DisplayDialog("Success", "All PlayerPrefs have been cleared.", "OK");
            }
        }

        [MenuItem("Tools/Maple Fighters/Show Player Data Info")]
        public static void ShowPlayerDataInfo()
        {
            var playersData = PlayerPrefs.GetString(PlayersStorageKey, "");
            var sessionData = PlayerPrefs.GetString(SessionStorageKey, "");
            
            string message = "=== Player Data ===\n";
            
            if (string.IsNullOrEmpty(playersData))
            {
                message += "Players: No data stored\n";
            }
            else
            {
                message += $"Players: {playersData.Length} characters stored\n";
                Debug.Log($"[PlayerDataCleaner] Players JSON:\n{playersData}");
            }
            
            if (string.IsNullOrEmpty(sessionData))
            {
                message += "Session: No session stored\n";
            }
            else
            {
                message += $"Session: {sessionData.Length} characters stored\n";
                Debug.Log($"[PlayerDataCleaner] Session JSON:\n{sessionData}");
            }
            
            message += "\nCheck Console for full JSON data.";
            
            EditorUtility.DisplayDialog("Player Data Info", message, "OK");
        }
    }
}
