using UnityEngine;

namespace Scripts.Gameplay.Player.Components
{
    /// <summary>
    /// Componente responsable de detectar si el jugador está en el suelo.
    /// Extrae esta responsabilidad del PlayerController (SRP).
    /// </summary>
    public class GroundDetector : MonoBehaviour
    {
        [Header("Ground Detection")]
        [SerializeField]
        [Tooltip("Radio del círculo de detección")]
        private float detectionRadius = 0.1f;

        [SerializeField]
        [Tooltip("Capas consideradas como suelo")]
        private LayerMask groundLayers;

        [SerializeField]
        [Tooltip("Punto desde donde se detecta el suelo (generalmente los pies)")]
        private Transform groundCheckPoint;

        [Header("Debug")]
        [SerializeField]
        private bool showDebugGizmos = true;

        [SerializeField]
        private Color groundedColor = Color.green;

        [SerializeField]
        private Color airborneColor = Color.red;

        private bool isGrounded;

        /// <summary>
        /// Indica si el jugador está actualmente en el suelo.
        /// </summary>
        public bool IsGrounded => isGrounded;

        private void FixedUpdate()
        {
            CheckGround();
        }

        /// <summary>
        /// Verifica si hay suelo debajo del punto de detección.
        /// </summary>
        public void CheckGround()
        {
            if (groundCheckPoint == null)
            {
                isGrounded = false;
                return;
            }

            isGrounded = Physics2D.OverlapCircle(
                groundCheckPoint.position,
                detectionRadius,
                groundLayers
            );
        }

        /// <summary>
        /// Fuerza una verificación inmediata del suelo.
        /// </summary>
        /// <returns>True si está en el suelo</returns>
        public bool CheckGroundImmediate()
        {
            CheckGround();
            return isGrounded;
        }

        /// <summary>
        /// Configura el detector en runtime.
        /// </summary>
        public void Configure(Transform checkPoint, float radius, LayerMask layers)
        {
            groundCheckPoint = checkPoint;
            detectionRadius = radius;
            groundLayers = layers;
        }

        /// <summary>
        /// Obtiene el collider del suelo actual (si existe).
        /// </summary>
        public Collider2D GetGroundCollider()
        {
            if (groundCheckPoint == null)
            {
                return null;
            }

            return Physics2D.OverlapCircle(
                groundCheckPoint.position,
                detectionRadius,
                groundLayers
            );
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || groundCheckPoint == null)
            {
                return;
            }

            Gizmos.color = isGrounded ? groundedColor : airborneColor;
            Gizmos.DrawWireSphere(groundCheckPoint.position, detectionRadius);
        }
#endif
    }
}
