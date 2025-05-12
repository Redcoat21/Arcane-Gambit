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
        [SerializeField] private HealthUI healthUI;

        private Vector2 lastMoveDirection;
        private int baseMaxHealth;

        private void Awake()
        {
            movementComponent ??= GetComponent<GroundMovementComponent>();
            healthComponent ??= GetComponent<HealthComponent>();
            animator ??= GetComponent<Animator>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            inventoryComponent ??= GetComponent<InventoryComponent>();
            if (inventoryComponent != null){
                inventoryComponent.OnInventoryChanged += ApplyInventoryModifiers;
            }
            healthUI ??= FindFirstObjectByType<HealthUI>();
        }

        private void Start()
        {
            baseMaxHealth = healthComponent.MaximumHealth;
            Debug.Log("Base Max Health: " + baseMaxHealth);

            Debug.Log("== Inventory Contents at Start ==");
            foreach (var item in inventoryComponent.GetItems())
            {
                Debug.Log($"{item.itemData.itemName} x{item.quantity}");
            }

            ApplyInventoryModifiers();
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

        private void ApplyInventoryModifiers()
        {
            float hpBonus = 0;

            foreach (var item in inventoryComponent.GetItems())
            {
                if (item.itemData.type == Type.Passive)
                {
                    hpBonus += item.itemData.hpModifier * item.quantity;
                }
            }

            int newMaxHP = Mathf.RoundToInt(baseMaxHealth + hpBonus);
            int currentHP = healthComponent.CurrentHealth;

            healthComponent.MaximumHealth = newMaxHP;
            healthComponent.CurrentHealth = Mathf.Clamp(currentHP, 0, newMaxHP);

            healthUI.UpdateUI();
        }
    }
}