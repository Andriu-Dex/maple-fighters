using UnityEngine;

namespace Scripts.Services.AuthenticatorApi
{
    /// <summary>
    /// Datos del usuario autenticado.
    /// </summary>
    public struct UserData
    {
        /// <summary>
        /// Identificador único del usuario (GUID o similar).
        /// </summary>
        public string id;

        /// <summary>
        /// Email del usuario (v2).
        /// </summary>
        public string email;

        /// <summary>
        /// Nombre del jugador.
        /// </summary>
        public string name;

        /// <summary>
        /// Clase del personaje seleccionado (Knight, Archer, Wizard).
        /// </summary>
        public string characterClass;

        public static UserData FromJson(string json)
        {
            return JsonUtility.FromJson<UserData>(json);
        }

        public static UserData Create(string id, string email, string name, string characterClass = null)
        {
            return new UserData
            {
                id = id,
                email = email,
                name = name,
                characterClass = characterClass
            };
        }
    }
}