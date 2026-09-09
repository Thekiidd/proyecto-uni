using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Platformer.Gameplay;
using static Platformer.Core.Simulation;
using Platformer.Model;
using Platformer.Core;
using UnityEngine.InputSystem;

namespace Platformer.Mechanics
{
    /// <summary>
    /// This is the main class used to implement control of the player.
    /// It is a superset of the AnimationController class, but is inlined to allow for any kind of customisation.
    /// </summary>
    public class PlayerController : KinematicObject
    {
        public AudioClip jumpAudio;
        public AudioClip respawnAudio;
        public AudioClip ouchAudio;

        /// <summary>
        /// Max horizontal speed of the player.
        /// </summary>
        public float maxSpeed = 7;
        /// <summary>
        /// Initial jump velocity at the start of a jump.
        /// </summary>
        public float jumpTakeOffSpeed = 7;

        public JumpState jumpState = JumpState.Grounded;
        private bool stopJump;
        /*internal new*/ public Collider2D collider2d;
        /*internal new*/ public AudioSource audioSource;
        public Health health;
        public bool controlEnabled = true;

        bool jump;
        Vector2 move;
        SpriteRenderer spriteRenderer;
        internal Animator animator;
        private Platformer.Core.CharacterData currentCharacterData;
        readonly PlatformerModel model = Simulation.GetModel<PlatformerModel>();

        private InputAction m_MoveAction;
        private InputAction m_JumpAction;
        private float animTimer;
        private int animFrameIndex;
        private bool isDying = false;
        private float deathAnimTimer = 0f;
        private int deathAnimFrame = 0;
        private Coroutine damageFlashCoroutine;

        // Temporizador para acciones espontáneas cuando está quieto (rascado, estiramiento, lamido, ladrido)
        private float idleActionTimer = 0f;
        private float nextIdleActionDelay = 6f;
        private float totalIdleStillTime = 0f;
        private Sprite[] activeSpecialFrames = null;
        private int specialFrameIdx = 0;
        private float specialAnimTimer = 0f;

        public Bounds Bounds => collider2d.bounds;

        void Awake()
        {
            health = GetComponent<Health>();
            audioSource = GetComponent<AudioSource>();
            collider2d = GetComponent<Collider2D>();
            spriteRenderer = GetComponent<SpriteRenderer>();
            animator = GetComponent<Animator>();

            m_MoveAction = InputSystem.actions.FindAction("Player/Move");
            m_JumpAction = InputSystem.actions.FindAction("Player/Jump");
            
            m_MoveAction.Enable();
            m_JumpAction.Enable();
        }

        protected override void Start()
        {
            base.Start();
            Platformer.UI.GameHUD.EnsureHUDExists();
            ApplySelectedCharacter();
        }

        public void ApplySelectedCharacter()
        {
            if (GameData.Instance != null && GameData.Instance.selectedCharacter != null)
            {
                var data = GameData.Instance.selectedCharacter;
                currentCharacterData = data;
                maxSpeed = data.moveSpeed;

                if (model != null)
                    model.jumpModifier = data.jumpStrength / 10f;

                if (spriteRenderer == null)
                    spriteRenderer = GetComponent<SpriteRenderer>();

                if (animator == null)
                    animator = GetComponent<Animator>();

                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                    if (data.idleSprite != null)
                        spriteRenderer.sprite = data.idleSprite;
                }

                if (data.animatorController != null && animator != null)
                {
                    animator.runtimeAnimatorController = data.animatorController;
                    animator.enabled = true;
                    animator.Rebind();
                    animator.Update(0f);
                }
                else
                {
                    // Si todavía no hay animator propio, apagamos el animator por defecto de Unity
                    if (animator != null) animator.enabled = false;
                    if (data.idleSprite != null && spriteRenderer != null)
                    {
                        spriteRenderer.sprite = data.idleSprite;
                    }
                }

                if (data.gravityModifier > 0)
                    gravityModifier = data.gravityModifier;
                else
                    gravityModifier = 1.5f;

                // Mantenemos la escala del jugador en (1, 1, 1)
                transform.localScale = Vector3.one;

                // Ajustamos el BoxCollider2D para que se adapte perfectamente al cuerpo del perro compacto
                if (collider2d is BoxCollider2D box)
                {
                    box.offset = new Vector2(0f, 0.35f);
                    box.size = new Vector2(0.8f, 0.65f);
                }

                Debug.Log($"[PlayerController] ✅ Personaje cargado: {data.characterName}");
            }
        }

        protected override void Update()
        {
            if (controlEnabled)
            {
                move.x = m_MoveAction.ReadValue<Vector2>().x;
                if (jumpState == JumpState.Grounded && m_JumpAction.WasPressedThisFrame())
                    jumpState = JumpState.PrepareToJump;
                else if (m_JumpAction.WasReleasedThisFrame())
                {
                    stopJump = true;
                    Schedule<PlayerStopJump>().player = this;
                }
            }
            else
            {
                move.x = 0;
            }
            UpdateJumpState();
            base.Update();
        }

        void UpdateJumpState()
        {
            jump = false;
            switch (jumpState)
            {
                case JumpState.PrepareToJump:
                    jumpState = JumpState.Jumping;
                    jump = true;
                    stopJump = false;
                    break;
                case JumpState.Jumping:
                    if (!IsGrounded)
                    {
                        Schedule<PlayerJumped>().player = this;
                        jumpState = JumpState.InFlight;
                    }
                    break;
                case JumpState.InFlight:
                    if (IsGrounded)
                    {
                        Schedule<PlayerLanded>().player = this;
                        jumpState = JumpState.Landed;
                    }
                    break;
                case JumpState.Landed:
                    jumpState = JumpState.Grounded;
                    break;
            }
        }

        protected override void ComputeVelocity()
        {
            if (jump && IsGrounded)
            {
                velocity.y = jumpTakeOffSpeed * model.jumpModifier;
                jump = false;
            }
            else if (stopJump)
            {
                stopJump = false;
                if (velocity.y > 0)
                {
                    velocity.y = velocity.y * model.jumpDeceleration;
                }
            }

            if (isDying)
            {
                if (currentCharacterData != null && currentCharacterData.deathFrames != null && currentCharacterData.deathFrames.Length > 0)
                {
                    deathAnimTimer += Time.deltaTime;
                    if (deathAnimTimer >= 0.12f)
                    {
                        deathAnimTimer = 0f;
                        if (deathAnimFrame < currentCharacterData.deathFrames.Length - 1)
                            deathAnimFrame++;
                    }
                    if (spriteRenderer != null)
                        spriteRenderer.sprite = currentCharacterData.deathFrames[deathAnimFrame];
                }
                targetVelocity = Vector2.zero;
                return;
            }

            if (move.x > 0.01f && spriteRenderer != null)
                spriteRenderer.flipX = false;
            else if (move.x < -0.01f && spriteRenderer != null)
                spriteRenderer.flipX = true;

            bool isMoving = Mathf.Abs(velocity.x) > 0.1f || Mathf.Abs(move.x) > 0.05f;

            if (isMoving)
            {
                // Al movernos se resetea el temporizador y se cancela cualquier acción especial
                idleActionTimer = 0f;
                totalIdleStillTime = 0f;
                activeSpecialFrames = null;
            }
            else
            {
                totalIdleStillTime += Time.deltaTime;
                idleActionTimer += Time.deltaTime;

                if (idleActionTimer >= nextIdleActionDelay)
                {
                    idleActionTimer = 0f;
                    nextIdleActionDelay = Random.Range(5f, 10f); // Entre 5 y 10 segundos aleatorios

                    string chosenTrigger;
                    Sprite[] chosenFrames = null;

                    // Si lleva más de 15 segundos totalmente quieto, puede echarse o dormir siesta
                    if (totalIdleStillTime > 15f && Random.value < 0.6f)
                    {
                        if (Random.value < 0.5f)
                        {
                            chosenTrigger = "lay_down";
                            chosenFrames = currentCharacterData?.layDownFrames;
                        }
                        else
                        {
                            chosenTrigger = "sleep";
                            chosenFrames = currentCharacterData?.sleepFrames;
                        }
                    }
                    else
                    {
                        // Acciones espontáneas cortas de perro feliz
                        int roll = Random.Range(0, 6);
                        switch (roll)
                        {
                            case 0:
                                chosenTrigger = "scratch";
                                chosenFrames = currentCharacterData?.scratchFrames;
                                break;
                            case 1:
                                chosenTrigger = "stretch";
                                chosenFrames = currentCharacterData?.stretchFrames;
                                break;
                            case 2:
                                chosenTrigger = "lick";
                                chosenFrames = currentCharacterData?.lickFrames;
                                break;
                            case 3:
                                chosenTrigger = "lick2";
                                chosenFrames = currentCharacterData?.lick2Frames;
                                break;
                            case 4:
                                chosenTrigger = "bark";
                                chosenFrames = currentCharacterData?.barkFrames;
                                break;
                            default:
                                chosenTrigger = "sit";
                                chosenFrames = currentCharacterData?.sitFrames;
                                break;
                        }
                    }

                    if (animator != null && animator.runtimeAnimatorController != null && animator.enabled)
                    {
                        animator.SetTrigger(chosenTrigger);
                    }
                    else if (chosenFrames != null && chosenFrames.Length > 0)
                    {
                        specialFrameIdx = 0;
                        specialAnimTimer = 0f;
                        activeSpecialFrames = chosenFrames;
                    }
                }
            }

            if (animator != null && animator.runtimeAnimatorController != null && animator.enabled)
            {
                animator.SetBool("grounded", IsGrounded);
                animator.SetFloat("velocityX", Mathf.Abs(velocity.x));
            }
            else if (spriteRenderer != null && currentCharacterData != null)
            {
                if (isMoving)
                {
                    animTimer += Time.deltaTime;
                    if (animTimer >= 0.08f)
                    {
                        animTimer = 0f;
                        animFrameIndex++;
                    }

                    if (currentCharacterData.runFrames != null && currentCharacterData.runFrames.Length > 0)
                        spriteRenderer.sprite = currentCharacterData.runFrames[animFrameIndex % currentCharacterData.runFrames.Length];
                    else if (currentCharacterData.runSprite != null)
                        spriteRenderer.sprite = currentCharacterData.runSprite;
                }
                else if (activeSpecialFrames != null && activeSpecialFrames.Length > 0)
                {
                    // Reproduciendo la acción espontánea especial
                    specialAnimTimer += Time.deltaTime;
                    if (specialAnimTimer >= 0.12f)
                    {
                        specialAnimTimer = 0f;
                        specialFrameIdx++;
                        if (specialFrameIdx >= activeSpecialFrames.Length)
                        {
                            // Terminó la acción espontánea, vuelve al reposo normal
                            activeSpecialFrames = null;
                        }
                    }

                    if (activeSpecialFrames != null && specialFrameIdx < activeSpecialFrames.Length)
                        spriteRenderer.sprite = activeSpecialFrames[specialFrameIdx];
                }
                else
                {
                    // Reposo normal tranquilo (respiración suave)
                    animTimer += Time.deltaTime;
                    if (animTimer >= 0.16f) // ~6 FPS suave
                    {
                        animTimer = 0f;
                        animFrameIndex++;
                    }

                    if (currentCharacterData.idleFrames != null && currentCharacterData.idleFrames.Length > 0)
                        spriteRenderer.sprite = currentCharacterData.idleFrames[animFrameIndex % currentCharacterData.idleFrames.Length];
                    else if (currentCharacterData.idleSprite != null)
                        spriteRenderer.sprite = currentCharacterData.idleSprite;
                }
            }

            targetVelocity = move * maxSpeed;
        }

        public void TriggerDeathAnimation()
        {
            isDying = true;
            deathAnimTimer = 0f;
            deathAnimFrame = 0;
            controlEnabled = false;
            velocity = Vector2.zero;
            targetVelocity = Vector2.zero;

            if (damageFlashCoroutine != null)
                StopCoroutine(damageFlashCoroutine);
            damageFlashCoroutine = StartCoroutine(FlashHurtRoutine());
        }

        public void ResetFromDeath()
        {
            isDying = false;
            controlEnabled = true;
            deathAnimFrame = 0;
            if (damageFlashCoroutine != null)
            {
                StopCoroutine(damageFlashCoroutine);
                damageFlashCoroutine = null;
            }
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }

        private IEnumerator FlashHurtRoutine()
        {
            for (int i = 0; i < 6; i++)
            {
                if (spriteRenderer != null)
                    spriteRenderer.color = new Color(1f, 0.3f, 0.3f, 0.6f); // Rojo translúcido de impacto
                yield return new WaitForSeconds(0.1f);
                if (spriteRenderer != null)
                    spriteRenderer.color = Color.white;
                yield return new WaitForSeconds(0.1f);
            }
        }

        public enum JumpState
        {
            Grounded,
            PrepareToJump,
            Jumping,
            InFlight,
            Landed
        }
    }
}