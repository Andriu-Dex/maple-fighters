using Scripts.Core.Domain.Interfaces;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Persistence
{
    /// <summary>
    /// Implementación de ISaveService usando PlayerPrefs de Unity.
    /// Útil como fallback o para plataformas donde JSON no es ideal.
    /// </summary>
    public class PlayerPrefsSaveService : ISaveService
    {
        private const string BoolTrueValue = "1";
        private const string BoolFalseValue = "0";

        public void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }

        public string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }

        public void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        public void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }

        public void SetBool(string key, bool value)
        {
            PlayerPrefs.SetString(key, value ? BoolTrueValue : BoolFalseValue);
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (!PlayerPrefs.HasKey(key))
            {
                return defaultValue;
            }

            var value = PlayerPrefs.GetString(key);
            return value == BoolTrueValue;
        }

        public bool HasKey(string key)
        {
            return PlayerPrefs.HasKey(key);
        }

        public void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        public void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        public void Save()
        {
            PlayerPrefs.Save();
        }
    }
}
