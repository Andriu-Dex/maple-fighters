using UnityEngine;

namespace Scripts.Gameplay.Player.Components
{
    /// <summary>
    /// Componente responsable de crear los efectos visuales del jugador.
    /// Extrae esta responsabilidad del PlayerController (SRP).
    /// </summary>
    public class PlayerEffects : MonoBehaviour
    {
        [Header("Rush Effect")]
        [SerializeField]
        [Tooltip("Prefab del efecto de rush/dash")]
        private GameObject rushEffectPrefab;

        [Header("Attack Effect")]
        [SerializeField]
        [Tooltip("Prefab del efecto de ataque")]
        private GameObject attackEffectPrefab;

        [SerializeField]
        [Tooltip("Punto donde aparece el efecto de ataque")]
        private Transform attackSpawnPoint;

        /// <summary>
        /// Referencia al transform del jugador para obtener posición y dirección.
        /// </summary>
        private Transform playerTransform;

        private void Awake()
        {
            playerTransform = transform;
        }

        /// <summary>
        /// Crea el efecto de rush en la posición del jugador.
        /// </summary>
        /// <param name="facingDirection">Dirección hacia la que mira el jugador</param>
        public void CreateRushEffect(Vector2 facingDirection)
        {
            if (rushEffectPrefab == null)
            {
                Debug.LogWarning("[PlayerEffects] Rush effect prefab not assigned.");
                return;
            }

            var effect = Instantiate(
                rushEffectPrefab,
                playerTransform.position,
                Quaternion.identity
            );

            ApplyDirectionToEffect(effect, facingDirection);
        }

        /// <summary>
        /// Crea el efecto de ataque en el punto de spawn configurado.
        /// </summary>
        /// <param name="facingDirection">Dirección hacia la que mira el jugador</param>
        public void CreateAttackEffect(Vector2 facingDirection)
        {
            if (attackEffectPrefab == null)
            {
                Debug.LogWarning("[PlayerEffects] Attack effect prefab not assigned.");
                return;
            }

            var spawnPosition = attackSpawnPoint != null 
                ? attackSpawnPoint.position 
                : playerTransform.position;
                
            var spawnRotation = attackSpawnPoint != null 
                ? attackSpawnPoint.rotation 
                : Quaternion.identity;

            var effect = Instantiate(
                attackEffectPrefab,
                spawnPosition,
                spawnRotation
            );

            ApplyDirectionToEffect(effect, facingDirection);
        }

        /// <summary>
        /// Crea un efecto genérico en una posición específica.
        /// </summary>
        /// <param name="effectPrefab">Prefab del efecto</param>
        /// <param name="position">Posición donde crear el efecto</param>
        /// <param name="facingDirection">Dirección del efecto</param>
        /// <returns>El GameObject del efecto creado</returns>
        public GameObject CreateEffect(GameObject effectPrefab, Vector3 position, Vector2 facingDirection)
        {
            if (effectPrefab == null)
            {
                return null;
            }

            var effect = Instantiate(effectPrefab, position, Quaternion.identity);
            ApplyDirectionToEffect(effect, facingDirection);
            return effect;
        }

        /// <summary>
        /// Aplica la dirección al efecto escalando en X.
        /// </summary>
        private void ApplyDirectionToEffect(GameObject effect, Vector2 direction)
        {
            if (effect == null)
            {
                return;
            }

            var scale = effect.transform.localScale;
            scale.x *= direction.x;
            effect.transform.localScale = scale;
        }

        /// <summary>
        /// Configura los prefabs de efectos en runtime.
        /// </summary>
        public void Configure(GameObject rushPrefab, GameObject attackPrefab, Transform attackPoint)
        {
            rushEffectPrefab = rushPrefab;
            attackEffectPrefab = attackPrefab;
            attackSpawnPoint = attackPoint;
        }

        /// <summary>
        /// Configura solo el punto de spawn del ataque.
        /// </summary>
        public void SetAttackSpawnPoint(Transform spawnPoint)
        {
            attackSpawnPoint = spawnPoint;
        }
    }
}
