using System;
using System.Collections.Generic;
using ScriptableObjects.Configurations;
using Scripts.Core.Domain.Interfaces;
using Scripts.Core.Infrastructure;
using Scripts.Editor;
using Scripts.Gameplay.Player.Components;
using Scripts.Gameplay.Player.States;
using Scripts.UI.Focus;
using UnityEngine;

#pragma warning disable 0109

namespace Scripts.Gameplay.Player
{
    /// <summary>
    /// Controlador principal del jugador.
    /// Gestiona el estado del jugador y coordina los diferentes componentes.
    /// 
    /// Componentes opcionales (agregar al mismo GameObject para mejor organización):
    /// - GroundDetector: Detección de suelo
    /// - PlayerEffects: Efectos visuales
    /// </summary>
    [RequireComponent(typeof(Collider2D))]
    public class PlayerController : MonoBehaviour
    {
        public Action<PlayerStates> PlayerStateChanged;

        [SerializeField]
        public CharacterClasses characterType;

        [Header("Debug")]
        [ViewOnly, SerializeField]
        private PlayerStates playerState = PlayerStates.Falling;

        [ViewOnly, SerializeField]
        private Direction direction;

        [Header("Ground")]
        [ViewOnly, SerializeField]
        private bool canMove = true;

        [SerializeField]
        private float overlapCircleRadius;

        [SerializeField]
        private LayerMask groundLayerMask;

        [SerializeField]
        private Transform groundTransform;

        [Header("Effects")]
        [SerializeField]
        private GameObject rushEffect;

        [SerializeField]
        private GameObject attackEffect;

        [SerializeField]
        private Transform attackEffectSpawnPosition;

        private Dictionary<PlayerStates, IPlayerStateBehaviour> playerStateBehaviours;

        private IPlayerStateBehaviour playerStateBehaviour;
        private IPlayerStateAnimator playerStateAnimator;

        private new Rigidbody2D rigidbody2D;

        private FocusStateController focusStateController;

        // Componentes opcionales (Fase 3 - Refactorización)
        private GroundDetector groundDetector;
        private PlayerEffects playerEffects;
        private IInputService inputService;

        private void Awake()
        {
            playerStateBehaviours = new Dictionary<PlayerStates, IPlayerStateBehaviour>
            {
                { PlayerStates.Idle, new PlayerIdleState(this) },
                { PlayerStates.Moving, new PlayerMovingState(this) },
                { PlayerStates.Jumping, new PlayerJumpingState(this) },
                { PlayerStates.Falling, new PlayerFallingState(this) },
                { PlayerStates.Dead, new PlayerDeadState() },
                { PlayerStates.Rope, new PlayerClimbState(this) },
                { PlayerStates.Ladder, new PlayerClimbState(this) },
                { PlayerStates.PrimaryAttack, new PlayerPrimaryAttackState(this) },
                { PlayerStates.SecondaryAttack, new PlayerSecondaryAttackState(this) }
            };
            playerStateBehaviour = playerStateBehaviours[playerState];
            focusStateController = FindObjectOfType<FocusStateController>();

            // Intentar obtener componentes opcionales
            TryGetOptionalComponents();
        }

        private void Start()
        {
            playerStateBehaviour?.OnStateEnter();
        }

        private void Update()
        {
            playerStateBehaviour?.OnStateUpdate();
        }

        private void FixedUpdate()
        {
            playerStateBehaviour?.OnStateFixedUpdate();
        }

        public PlayerProperties GetProperties()
        {
            return PlayerConfiguration.GetInstance().PlayerProperties;
        }

        public PlayerKeyboard GetKeyboardSettings()
        {
            return PlayerConfiguration.GetInstance().PlayerKeyboard;
        }

        public void SetPlayerStateAnimator(IPlayerStateAnimator playerStateAnimator)
        {
            this.playerStateAnimator = playerStateAnimator;
        }

        public IPlayerStateAnimator GetPlayerStateAnimator()
        {
            return playerStateAnimator;
        }

        public void SetPlayerState(PlayerStates newPlayerState)
        {
            if (playerState != newPlayerState)
            {
                playerStateBehaviour.OnStateExit();
                playerStateBehaviour = playerStateBehaviours[newPlayerState];
                playerStateBehaviour.OnStateEnter();

                playerStateAnimator?.SetPlayerState(newPlayerState);
                playerState = newPlayerState;

                PlayerStateChanged?.Invoke(newPlayerState);
            }
        }

        public PlayerStates GetPlayerState()
        {
            return playerState;
        }

        public void ChangeDirection(Direction direction)
        {
            this.direction = direction;

            var x = Mathf.Abs(transform.localScale.x);
            var y = transform.localScale.y;
            var z = transform.localScale.z;

            if (direction == Direction.Left)
            {
                transform.localScale = new Vector3(x, y, z);
            }
            else if (direction == Direction.Right)
            {
                transform.localScale = new Vector3(-x, y, z);
            }
        }

        public void SetDirection(float value)
        {
            direction = value > 0 ? Direction.Left : Direction.Right;

            var y = transform.localScale.y;
            var z = transform.localScale.z;

            transform.localScale = new Vector3(value, y, z);
        }

        public Vector2 GetDirection()
        {
            return direction == Direction.Left ? Vector3.left : Vector3.right;
        }

        public CharacterClasses GetCharacterType()
        {
            return characterType;
        }

        public void Bounce(Vector2 force)
        {
            if (rigidbody2D == null)
            {
                var collider = GetComponent<Collider2D>();
                rigidbody2D = collider.attachedRigidbody;
            }

            rigidbody2D.AddForce(force, ForceMode2D.Impulse);
        }

        public void CreateRushEffect()
        {
            // Usar componente PlayerEffects si está disponible
            if (playerEffects != null)
            {
                playerEffects.CreateRushEffect(GetDirection());
                return;
            }

            // Fallback al comportamiento original
            var position = transform.position;
            var rotation = Quaternion.identity;
            var dir = GetDirection();
            var effect = Instantiate(rushEffect, position, rotation);

            var x = effect.transform.localScale.x * dir.x;
            var y = effect.transform.localScale.y;
            var z = effect.transform.localScale.z;

            effect.transform.localScale = new Vector3(x, y, z);
        }

        public void CreateAttackEffect()
        {
            // Usar componente PlayerEffects si está disponible
            if (playerEffects != null)
            {
                playerEffects.CreateAttackEffect(GetDirection());
                return;
            }

            // Fallback al comportamiento original
            var position = attackEffectSpawnPosition.position;
            var rotation = attackEffectSpawnPosition.rotation;
            var dir = GetDirection();
            var effect = Instantiate(attackEffect, position, rotation);

            var x = effect.transform.localScale.x * dir.x;
            var y = effect.transform.localScale.y;
            var z = effect.transform.localScale.z;

            effect.transform.localScale = new Vector3(x, y, z);
        }

        public void SetCanMove(bool canMove)
        {
            this.canMove = canMove;
        }

        public bool CanMove()
        {
            if (focusStateController?.GetFocusState() != UIFocusState.Game)
            {
                return false;
            }

            return canMove;
        }

        public bool IsMoving()
        {
            // Usar IInputService si está disponible
            if (inputService != null)
            {
                return Mathf.Abs(inputService.HorizontalRaw) > 0;
            }

            // Fallback al comportamiento original
            var horizontal = Utils.GetAxis(Axes.Horizontal, isRaw: true);
            return Mathf.Abs(horizontal) > 0;
        }

        public bool IsGrounded()
        {
            // Usar componente GroundDetector si está disponible
            if (groundDetector != null)
            {
                return groundDetector.IsGrounded;
            }

            // Fallback al comportamiento original
            var isGrounded = false;

            if (groundTransform != null)
            {
                var position = groundTransform.position;
                var radius = overlapCircleRadius;
                var layerMask = groundLayerMask;

                isGrounded = Physics2D.OverlapCircle(position, radius, layerMask);
            }

            return isGrounded;
        }

        #region Optional Components (Fase 3)

        /// <summary>
        /// Intenta obtener los componentes opcionales del mismo GameObject o hijos.
        /// </summary>
        private void TryGetOptionalComponents()
        {
            // Intentar obtener GroundDetector
            groundDetector = GetComponent<GroundDetector>();
            if (groundDetector == null)
            {
                groundDetector = GetComponentInChildren<GroundDetector>();
            }

            // Intentar obtener PlayerEffects
            playerEffects = GetComponent<PlayerEffects>();
            if (playerEffects == null)
            {
                playerEffects = GetComponentInChildren<PlayerEffects>();
            }

            // Intentar obtener IInputService del ServiceLocator
            ServiceLocator.TryGet(out inputService);

            // Si hay GroundDetector, configurarlo con los valores del Inspector
            if (groundDetector != null)
            {
                groundDetector.Configure(groundTransform, overlapCircleRadius, groundLayerMask);
            }

            // Si hay PlayerEffects, configurarlo con los prefabs
            if (playerEffects != null)
            {
                playerEffects.Configure(rushEffect, attackEffect, attackEffectSpawnPosition);
            }
        }

        /// <summary>
        /// Obtiene el servicio de input actual.
        /// </summary>
        public IInputService GetInputService()
        {
            return inputService;
        }

        #endregion
    }
}