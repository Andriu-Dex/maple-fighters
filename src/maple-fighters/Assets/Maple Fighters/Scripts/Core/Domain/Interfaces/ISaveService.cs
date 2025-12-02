namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para servicios de persistencia de datos.
    /// Permite guardar y cargar datos de forma agnóstica al almacenamiento.
    /// </summary>
    public interface ISaveService
    {
        /// <summary>
        /// Guarda un valor string con la clave especificada.
        /// </summary>
        void SetString(string key, string value);

        /// <summary>
        /// Obtiene un valor string por su clave.
        /// </summary>
        /// <param name="key">Clave del valor.</param>
        /// <param name="defaultValue">Valor por defecto si no existe.</param>
        /// <returns>El valor guardado o el valor por defecto.</returns>
        string GetString(string key, string defaultValue = "");

        /// <summary>
        /// Guarda un valor entero con la clave especificada.
        /// </summary>
        void SetInt(string key, int value);

        /// <summary>
        /// Obtiene un valor entero por su clave.
        /// </summary>
        int GetInt(string key, int defaultValue = 0);

        /// <summary>
        /// Guarda un valor float con la clave especificada.
        /// </summary>
        void SetFloat(string key, float value);

        /// <summary>
        /// Obtiene un valor float por su clave.
        /// </summary>
        float GetFloat(string key, float defaultValue = 0f);

        /// <summary>
        /// Guarda un valor booleano con la clave especificada.
        /// </summary>
        void SetBool(string key, bool value);

        /// <summary>
        /// Obtiene un valor booleano por su clave.
        /// </summary>
        bool GetBool(string key, bool defaultValue = false);

        /// <summary>
        /// Verifica si existe una clave.
        /// </summary>
        bool HasKey(string key);

        /// <summary>
        /// Elimina una clave y su valor.
        /// </summary>
        void DeleteKey(string key);

        /// <summary>
        /// Elimina todos los datos guardados.
        /// </summary>
        void DeleteAll();

        /// <summary>
        /// Persiste los cambios al almacenamiento.
        /// </summary>
        void Save();
    }
}
