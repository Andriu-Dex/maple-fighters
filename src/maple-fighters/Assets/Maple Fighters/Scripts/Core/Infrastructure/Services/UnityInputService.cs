using Scripts.Core.Domain.Interfaces;
using UnityEngine;

namespace Scripts.Core.Infrastructure.Services
{
    /// <summary>
    /// Implementación de IInputService que usa el sistema de input de Unity.
    /// </summary>
    public class UnityInputService : IInputService
    {
        private const string HorizontalAxis = "Horizontal";
        private const string VerticalAxis = "Vertical";

        public float GetAxis(string axisName)
        {
            return Input.GetAxis(axisName);
        }

        public float GetAxisRaw(string axisName)
        {
            return Input.GetAxisRaw(axisName);
        }

        public bool GetKeyDown(KeyCode key)
        {
            return Input.GetKeyDown(key);
        }

        public bool GetKey(KeyCode key)
        {
            return Input.GetKey(key);
        }

        public bool GetKeyUp(KeyCode key)
        {
            return Input.GetKeyUp(key);
        }

        public float Horizontal => Input.GetAxis(HorizontalAxis);

        public float HorizontalRaw => Input.GetAxisRaw(HorizontalAxis);

        public float Vertical => Input.GetAxis(VerticalAxis);

        public float VerticalRaw => Input.GetAxisRaw(VerticalAxis);
    }
}
