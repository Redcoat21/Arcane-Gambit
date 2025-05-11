using System;
using Components.Health;
using Components.Movements;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField] private GroundMovementComponent movementComponent;
        [SerializeField] private HealthComponent healthComponent;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private InventoryComponent inventoryComponent;    

        private Vector2 lastMoveDirection;

        private void Awake()
        {
            movementComponent ??= GetComponent<GroundMovementComponent>();
            healthComponent ??= GetComponent<HealthComponent>();
            animator ??= GetComponent<Animator>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            inventoryComponent ??= GetComponent<InventoryComponent>();
        }

        private void FixedUpdate()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            movementComponent?.Move(input);

            UpdateAnimator(input);
        }

        private void UpdateAnimator(Vector2 input)
        {
            bool isMoving = input != Vector2.zero;

            if (isMoving)
            {
                lastMoveDirection = input.normalized;

                // Flip sprite for left/right
                if (Mathf.Abs(input.x) > 0.1f)
                {
                    spriteRenderer.flipX = input.x < 0;
                }
            }

            animator.SetFloat("moveX", lastMoveDirection.x);
            animator.SetFloat("moveY", lastMoveDirection.y);
            animator.SetBool("isMoving", isMoving);
        }
    }
}