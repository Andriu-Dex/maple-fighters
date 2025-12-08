using System;
using System.Collections.Generic;
using System.IO;
using Scripts.Core.Domain.Interfaces;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación de ISaveService que guarda datos en formato JSON.
    /// En WebGL usa PlayerPrefs (IndexedDB), en otras plataformas usa archivos.
    /// </summary>
    public class JsonSaveService : ISaveService
    {
        private const string SaveFileName = "game_save_data.json";
        private const string PlayerPrefsKey = "JsonSaveService_Data";
        
        private Dictionary<string, object> data;
        private readonly string savePath;
        private bool isDirty;
        private readonly bool usePlayerPrefs;

        public JsonSaveService()
        {
            // En WebGL no podemos usar el sistema de archivos, usamos PlayerPrefs (IndexedDB)
#if UNITY_WEBGL && !UNITY_EDITOR
            usePlayerPrefs = true;
            savePath = string.Empty;
            Debug.Log("[JsonSaveService] WebGL detectado - usando PlayerPrefs (IndexedDB)");
#else
            usePlayerPrefs = false;
            savePath = Path.Combine(Application.persistentDataPath, SaveFileName);
            Debug.Log($"[JsonSaveService] Usando archivos - path: {savePath}");
#endif
            data = new Dictionary<string, object>();
            LoadFromFile();
        }

        public void SetString(string key, string value)
        {
            data[key] = value;
            isDirty = true;
        }

        public string GetString(string key, string defaultValue = "")
        {
            if (data.TryGetValue(key, out var value))
            {
                return value?.ToString() ?? defaultValue;
            }
            return defaultValue;
        }

        public void SetInt(string key, int value)
        {
            data[key] = value;
            isDirty = true;
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value is int intValue)
                {
                    return intValue;
                }
                if (value is long longValue)
                {
                    return (int)longValue;
                }
                if (int.TryParse(value?.ToString(), out var parsed))
                {
                    return parsed;
                }
            }
            return defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            data[key] = value;
            isDirty = true;
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value is float floatValue)
                {
                    return floatValue;
                }
                if (value is double doubleValue)
                {
                    return (float)doubleValue;
                }
                if (float.TryParse(value?.ToString(), out var parsed))
                {
                    return parsed;
                }
            }
            return defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            data[key] = value;
            isDirty = true;
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (data.TryGetValue(key, out var value))
            {
                if (value is bool boolValue)
                {
                    return boolValue;
                }
                if (bool.TryParse(value?.ToString(), out var parsed))
                {
                    return parsed;
                }
            }
            return defaultValue;
        }

        public bool HasKey(string key)
        {
            return data.ContainsKey(key);
        }

        public void DeleteKey(string key)
        {
            if (data.Remove(key))
            {
                isDirty = true;
            }
        }

        public void DeleteAll()
        {
            data.Clear();
            isDirty = true;
        }

        public void Save()
        {
            if (!isDirty)
            {
                return;
            }

            try
            {
                var json = SerializeDictionary(data);
                
                if (usePlayerPrefs)
                {
                    // WebGL: usar PlayerPrefs (se mapea a IndexedDB)
                    PlayerPrefs.SetString(PlayerPrefsKey, json);
                    PlayerPrefs.Save();
                    Debug.Log($"[JsonSaveService] Data saved to PlayerPrefs (WebGL)");
                }
                else
                {
                    // Otras plataformas: usar archivo
                    File.WriteAllText(savePath, json);
                    Debug.Log($"[JsonSaveService] Data saved to: {savePath}");
                }
                
                isDirty = false;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveService] Error saving data: {ex.Message}");
            }
        }

        private void LoadFromFile()
        {
            try
            {
                string json = string.Empty;
                
                if (usePlayerPrefs)
                {
                    // WebGL: leer de PlayerPrefs
                    if (PlayerPrefs.HasKey(PlayerPrefsKey))
                    {
                        json = PlayerPrefs.GetString(PlayerPrefsKey);
                        Debug.Log($"[JsonSaveService] Data loaded from PlayerPrefs (WebGL)");
                    }
                    else
                    {
                        Debug.Log("[JsonSaveService] No PlayerPrefs data found, starting fresh.");
                    }
                }
                else
                {
                    // Otras plataformas: leer de archivo
                    if (File.Exists(savePath))
                    {
                        json = File.ReadAllText(savePath);
                        Debug.Log($"[JsonSaveService] Data loaded from: {savePath}");
                    }
                    else
                    {
                        Debug.Log("[JsonSaveService] No save file found, starting fresh.");
                    }
                }
                
                if (!string.IsNullOrEmpty(json))
                {
                    data = DeserializeDictionary(json);
                }
                else
                {
                    data = new Dictionary<string, object>();
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[JsonSaveService] Error loading data: {ex.Message}");
                data = new Dictionary<string, object>();
            }
        }

        private string SerializeDictionary(Dictionary<string, object> dict)
        {
            var entries = new List<string>();
            
            foreach (var kvp in dict)
            {
                var valueJson = SerializeValue(kvp.Value);
                entries.Add($"\"{kvp.Key}\": {valueJson}");
            }
            
            return "{\n  " + string.Join(",\n  ", entries) + "\n}";
        }

        private string SerializeValue(object value)
        {
            if (value == null)
            {
                return "null";
            }
            if (value is string str)
            {
                return $"\"{EscapeString(str)}\"";
            }
            if (value is bool b)
            {
                return b ? "true" : "false";
            }
            if (value is int || value is long || value is float || value is double)
            {
                return value.ToString().Replace(",", ".");
            }
            
            return $"\"{value}\"";
        }

        private string EscapeString(string str)
        {
            return str
                .Replace("\\", "\\\\")
                .Replace("\"", "\\\"")
                .Replace("\n", "\\n")
                .Replace("\r", "\\r")
                .Replace("\t", "\\t");
        }

        private Dictionary<string, object> DeserializeDictionary(string json)
        {
            var result = new Dictionary<string, object>();
            
            if (string.IsNullOrEmpty(json))
            {
                return result;
            }

            // Parseo simple de JSON para Dictionary<string, object>
            json = json.Trim();
            if (!json.StartsWith("{") || !json.EndsWith("}"))
            {
                return result;
            }

            json = json.Substring(1, json.Length - 2).Trim();
            
            if (string.IsNullOrEmpty(json))
            {
                return result;
            }

            var depth = 0;
            var inString = false;
            var currentEntry = "";
            
            for (int i = 0; i < json.Length; i++)
            {
                var c = json[i];
                
                if (c == '"' && (i == 0 || json[i - 1] != '\\'))
                {
                    inString = !inString;
                }
                
                if (!inString)
                {
                    if (c == '{' || c == '[') depth++;
                    if (c == '}' || c == ']') depth--;
                    
                    if (c == ',' && depth == 0)
                    {
                        ParseEntry(currentEntry.Trim(), result);
                        currentEntry = "";
                        continue;
                    }
                }
                
                currentEntry += c;
            }
            
            if (!string.IsNullOrEmpty(currentEntry.Trim()))
            {
                ParseEntry(currentEntry.Trim(), result);
            }
            
            return result;
        }

        private void ParseEntry(string entry, Dictionary<string, object> result)
        {
            var colonIndex = entry.IndexOf(':');
            if (colonIndex <= 0) return;
            
            var key = entry.Substring(0, colonIndex).Trim().Trim('"');
            var valueStr = entry.Substring(colonIndex + 1).Trim();
            
            result[key] = ParseValue(valueStr);
        }

        private object ParseValue(string valueStr)
        {
            if (valueStr == "null")
            {
                return null;
            }
            if (valueStr == "true")
            {
                return true;
            }
            if (valueStr == "false")
            {
                return false;
            }
            if (valueStr.StartsWith("\"") && valueStr.EndsWith("\""))
            {
                return UnescapeString(valueStr.Substring(1, valueStr.Length - 2));
            }
            if (int.TryParse(valueStr, out var intVal))
            {
                return intVal;
            }
            if (float.TryParse(valueStr.Replace(".", ","), out var floatVal))
            {
                return floatVal;
            }
            
            return valueStr;
        }

        private string UnescapeString(string str)
        {
            return str
                .Replace("\\\"", "\"")
                .Replace("\\\\", "\\")
                .Replace("\\n", "\n")
                .Replace("\\r", "\r")
                .Replace("\\t", "\t");
        }

        [Serializable]
        private class SaveDataWrapper
        {
            public Dictionary<string, object> Data;
        }
    }
}
