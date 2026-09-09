using System.Collections.Generic;
using Platformer.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Platformer.TopDown
{
    /// <summary>
    /// Controlador de movimiento Top-Down (2D cenital) para el perro en la aldea.
    /// Movimiento en 8 direcciones con W, A, S, D o flechas, sin gravedad.
    /// </summary>
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class TopDownDogController : MonoBehaviour
    {
        [Header("Movimiento")]
        public float moveSpeed = 6f;
        public bool controlEnabled = true;

        [Header("Referencias")]
        public SpriteRenderer spriteRenderer;
        public Animator animator;
        public CharacterData characterData;

        private Rigidbody2D rb;
        private Vector2 movementInput;
        private float animTimer;
        private int animFrameIndex;
        private InputAction moveAction;

        // Temporizadores para acciones espontáneas en reposo (5-10s entre acciones)
        private float idleActionTimer;
        private float nextIdleActionDelay = 6f;
        private float totalIdleStillTime = 0f;
        private Sprite[] activeSpecialFrames;
        private int specialFrameIdx;
        private float specialAnimTimer;

        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();
            if (animator == null)
            {
                animator = GetComponent<Animator>();
                if (animator == null) animator = gameObject.AddComponent<Animator>();
            }

            // Configurar Rigidbody2D para movimiento cenital
            rb.gravityScale = 0f;
            rb.constraints = RigidbodyConstraints2D.FreezeRotation;

            try
            {
                moveAction = InputSystem.actions?.FindAction("Player/Move");
                moveAction?.Enable();
            }
            catch { }
        }

        private void Start()
        {
            ApplyCharacterData();
        }

        public void ApplyCharacterData()
        {
            if (GameData.Instance != null && GameData.Instance.selectedCharacter != null)
            {
                characterData = GameData.Instance.selectedCharacter;
            }
            #if UNITY_EDITOR
            else if (characterData == null)
            {
                characterData = UnityEditor.AssetDatabase.LoadAssetAtPath<CharacterData>("Assets/Characters/Runner.asset");
            }
            #endif

            if (characterData != null)
            {
                moveSpeed = Mathf.Max(5f, characterData.moveSpeed * 0.7f);
                if (spriteRenderer != null)
                {
                    spriteRenderer.color = Color.white;
                    if (characterData.idleSprite != null)
                        spriteRenderer.sprite = characterData.idleSprite;
                }
                if (animator != null && characterData.animatorController != null)
                {
                    animator.runtimeAnimatorController = characterData.animatorController;
                    animator.enabled = true;
                    animator.Rebind();
                    animator.Update(0f);
                }
            }
        }

        private void Update()
        {
            if (!controlEnabled)
            {
                movementInput = Vector2.zero;
                UpdateAnimation(false);
                return;
            }

            // Lectura de entrada: Input System nuevo (Action, Keyboard o Gamepad)
            Vector2 input = Vector2.zero;
            if (moveAction != null && moveAction.enabled)
            {
                input = moveAction.ReadValue<Vector2>();
            }

            if (input == Vector2.zero && Keyboard.current != null)
            {
                var kb = Keyboard.current;
                float x = 0f;
                float y = 0f;
                if (kb.wKey.isPressed || kb.upArrowKey.isPressed) y += 1f;
                if (kb.sKey.isPressed || kb.downArrowKey.isPressed) y -= 1f;
                if (kb.aKey.isPressed || kb.leftArrowKey.isPressed) x -= 1f;
                if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) x += 1f;
                input = new Vector2(x, y);
            }

            if (input == Vector2.zero && Gamepad.current != null)
            {
                input = Gamepad.current.leftStick.ReadValue();
                if (input.magnitude < 0.15f)
                    input = Gamepad.current.dpad.ReadValue();
            }

            movementInput = input.normalized;

            // Orientación horizontal del sprite (mirar a la izquierda o derecha)
            if (movementInput.x > 0.05f && spriteRenderer != null)
                spriteRenderer.flipX = false;
            else if (movementInput.x < -0.05f && spriteRenderer != null)
                spriteRenderer.flipX = true;

            // Orden de capa por profundidad Y (efecto RPG: delante o detrás de casas/árboles)
            // Base en 500 para estar siempre por encima del fondo (-1000) y ordenar entre objetos
            if (spriteRenderer != null)
            {
                spriteRenderer.sortingOrder = 500 - Mathf.RoundToInt(transform.position.y * 10f);
            }

            UpdateAnimation(movementInput.sqrMagnitude > 0.01f);
        }

        private void FixedUpdate()
        {
            Vector2 targetVel = movementInput * moveSpeed;
            #if UNITY_6000_0_OR_NEWER
            rb.linearVelocity = targetVel;
            #else
            rb.velocity = targetVel;
            #endif
        }

        private void UpdateAnimation(bool isMoving)
        {
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

                    // Si lleva más de 15 segundos totalmente quieto, puede descansar profundamente
                    if (totalIdleStillTime > 15f && Random.value < 0.6f)
                    {
                        if (Random.value < 0.5f)
                        {
                            chosenTrigger = "lay_down";
                            chosenFrames = characterData?.layDownFrames;
                        }
                        else
                        {
                            chosenTrigger = "sleep";
                            chosenFrames = characterData?.sleepFrames;
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
                                chosenFrames = characterData?.scratchFrames;
                                break;
                            case 1:
                                chosenTrigger = "stretch";
                                chosenFrames = characterData?.stretchFrames;
                                break;
                            case 2:
                                chosenTrigger = "lick";
                                chosenFrames = characterData?.lickFrames;
                                break;
                            case 3:
                                chosenTrigger = "lick2";
                                chosenFrames = characterData?.lick2Frames;
                                break;
                            case 4:
                                chosenTrigger = "bark";
                                chosenFrames = characterData?.barkFrames;
                                break;
                            default:
                                chosenTrigger = "sit";
                                chosenFrames = characterData?.sitFrames;
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
                // En vista cenital usamos la magnitud completa del movimiento (W, A, S, D)
                animator.SetFloat("velocityX", isMoving ? Mathf.Max(3f, movementInput.magnitude * 3f) : 0f);
                animator.SetBool("grounded", true);
            }
            else if (characterData != null && spriteRenderer != null)
            {
                if (isMoving)
                {
                    Sprite[] moveFrames = (characterData.runFrames != null && characterData.runFrames.Length > 0)
                        ? characterData.runFrames
                        : characterData.walkFrames;

                    if (moveFrames != null && moveFrames.Length > 0)
                    {
                        animTimer += Time.deltaTime;
                        if (animTimer >= 0.08f)
                        {
                            animTimer = 0f;
                            animFrameIndex = (animFrameIndex + 1) % moveFrames.Length;
                        }
                        if (moveFrames[animFrameIndex] != null)
                            spriteRenderer.sprite = moveFrames[animFrameIndex];
                    }
                }
                else if (activeSpecialFrames != null && activeSpecialFrames.Length > 0)
                {
                    specialAnimTimer += Time.deltaTime;
                    if (specialAnimTimer >= 0.12f)
                    {
                        specialAnimTimer = 0f;
                        specialFrameIdx++;
                        if (specialFrameIdx >= activeSpecialFrames.Length)
                        {
                            activeSpecialFrames = null;
                            specialFrameIdx = 0;
                        }
                    }
                    if (activeSpecialFrames != null && specialFrameIdx < activeSpecialFrames.Length && activeSpecialFrames[specialFrameIdx] != null)
                    {
                        spriteRenderer.sprite = activeSpecialFrames[specialFrameIdx];
                    }
                }
                else if (characterData.idleFrames != null && characterData.idleFrames.Length > 0)
                {
                    animTimer += Time.deltaTime;
                    if (animTimer >= 0.15f) // 6-7 FPS para respiración suave
                    {
                        animTimer = 0f;
                        animFrameIndex = (animFrameIndex + 1) % characterData.idleFrames.Length;
                    }
                    if (characterData.idleFrames[animFrameIndex] != null)
                        spriteRenderer.sprite = characterData.idleFrames[animFrameIndex];
                }
            }
        }
    }
}
