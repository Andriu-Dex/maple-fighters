using UnityEngine;

namespace Scripts.Core.Domain.Interfaces
{
    /// <summary>
    /// Interface para el servicio de input del juego.
    /// Abstrae el acceso al input de Unity para facilitar testing y desacoplamiento.
    /// </summary>
    public interface IInputService
    {
        /// <summary>
        /// Obtiene el valor del eje especificado.
        /// </summary>
        /// <param name="axisName">Nombre del eje (Horizontal, Vertical, etc.)</param>
        /// <returns>Valor del eje entre -1 y 1</returns>
        float GetAxis(string axisName);

        /// <summary>
        /// Obtiene el valor raw del eje especificado (sin suavizado).
        /// </summary>
        /// <param name="axisName">Nombre del eje</param>
        /// <returns>Valor del eje (-1, 0, o 1)</returns>
        float GetAxisRaw(string axisName);

        /// <summary>
        /// Verifica si una tecla fue presionada este frame.
        /// </summary>
        /// <param name="key">Código de la tecla</param>
        /// <returns>True si la tecla fue presionada</returns>
        bool GetKeyDown(KeyCode key);

        /// <summary>
        /// Verifica si una tecla está siendo mantenida.
        /// </summary>
        /// <param name="key">Código de la tecla</param>
        /// <returns>True si la tecla está presionada</returns>
        bool GetKey(KeyCode key);

        /// <summary>
        /// Verifica si una tecla fue soltada este frame.
        /// </summary>
        /// <param name="key">Código de la tecla</param>
        /// <returns>True si la tecla fue soltada</returns>
        bool GetKeyUp(KeyCode key);

        /// <summary>
        /// Obtiene el valor del eje horizontal.
        /// </summary>
        float Horizontal { get; }

        /// <summary>
        /// Obtiene el valor del eje horizontal sin suavizado.
        /// </summary>
        float HorizontalRaw { get; }

        /// <summary>
        /// Obtiene el valor del eje vertical.
        /// </summary>
        float Vertical { get; }

        /// <summary>
        /// Obtiene el valor del eje vertical sin suavizado.
        /// </summary>
        float VerticalRaw { get; }
    }
}
