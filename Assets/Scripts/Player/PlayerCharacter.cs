using System;
using System.Collections;
using Components.Attack;
using Components.ElementalDamage;
using Components.Health;
using Components.Mana;
using Components.MeleeDamage;
using Components.Movements;
using Components.RangedDamage;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Player
{
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField] private GroundMovementComponent movementComponent;
        [SerializeField] private GroundMovementUI movementUI;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private InventoryComponent inventoryComponent;
        [SerializeField] private HealthComponent healthComponent;  
        [SerializeField] private HealthUI healthUI;
        [SerializeField] private ManaComponent manaComponent;
        [SerializeField] private ManaUI manaUI;
        [SerializeField] private AttackComponent attackComponent;
        [SerializeField] private AttackUI attackUI;
        [SerializeField] private MeleeDamageComponent meleeDamageComponent;
        [SerializeField] private MeleeDamageUI meleeDamageUI;
        [SerializeField] private RangedDamageComponent rangedDamageComponent;
        [SerializeField] private RangedDamageUI rangedDamageUI;
        [SerializeField] private ElementalDamageComponent elementalDamageComponent;
        [SerializeField] private ElementalDamageUI elementalDamageUI;
        [SerializeField] private Image weapon1ImageUI;
        [SerializeField] private Image weapon1InventoryImageUI;
        [SerializeField] private CurrencyComponent currencyComponent;

        private Vector2 lastMoveDirection;
        private int baseMaxHealth;
        private int baseMaxMana;
        private float baseSpeed;
        private int baseAttack;
        private float baseMelee;
        private float baseRanged;
        private float baseElemental;
        public WeaponData weapon1;
        public WeaponData weapon2;
        public SpellData spell;
        public ConsumableData consumable;

        private void Awake()
        {
            movementComponent ??= GetComponent<GroundMovementComponent>();
            healthComponent ??= GetComponent<HealthComponent>();
            manaComponent ??= GetComponent<ManaComponent>();
            attackComponent ??= GetComponent<AttackComponent>();
            meleeDamageComponent ??= GetComponent<MeleeDamageComponent>();
            rangedDamageComponent ??= GetComponent<RangedDamageComponent>();
            elementalDamageComponent ??= GetComponent<ElementalDamageComponent>();
            animator ??= GetComponent<Animator>();
            spriteRenderer ??= GetComponent<SpriteRenderer>();
            inventoryComponent ??= GetComponent<InventoryComponent>();
            currencyComponent ??= GetComponent<CurrencyComponent>();
            if (inventoryComponent != null){
                inventoryComponent.OnInventoryChanged += ApplyInventoryModifiers;
            }
            healthUI ??= FindFirstObjectByType<HealthUI>();
            manaUI ??= FindFirstObjectByType<ManaUI>();
            attackUI ??= FindFirstObjectByType<AttackUI>();
            movementUI ??= FindFirstObjectByType<GroundMovementUI>();
            meleeDamageUI ??= FindFirstObjectByType<MeleeDamageUI>();
            rangedDamageUI ??= FindFirstObjectByType<RangedDamageUI>();
            elementalDamageUI ??= FindFirstObjectByType<ElementalDamageUI>();
        }

        private IEnumerator Start()
        {
            yield return null;
            if (SelectedWeaponStorage.selectedWeapon != null)
            {
                weapon1 = SelectedWeaponStorage.selectedWeapon;

                if (weapon1ImageUI != null)
                {
                    weapon1ImageUI.sprite = weapon1.weaponSprite;
                    weapon1InventoryImageUI.sprite = weapon1.weaponSprite;
                }

                Debug.Log($"Equipped weapon1: {weapon1.weaponName}");

                // Clear stored data after equipping
                SelectedWeaponStorage.selectedWeapon = null;
            }

            attackComponent.CurrentWeapon = weapon1;

            baseMaxHealth = healthComponent.MaximumHealth;
            Debug.Log("Base Max Health: " + baseMaxHealth);

            baseMaxMana = manaComponent.MaximumMana;
            Debug.Log("Base Max Mana: " + baseMaxHealth);

            baseSpeed = movementComponent.MoveSpeed;
            Debug.Log("Base Speed: " + baseMaxHealth);

            baseAttack = attackComponent.BaseAttackDamage;
            Debug.Log("Base Attack: " + baseAttack);

            baseMelee = meleeDamageComponent.MeleeMultiplier;
            Debug.Log("Base Melee Damage: " + baseMelee);

            baseRanged = rangedDamageComponent.RangedMultiplier;
            Debug.Log("Base Ranged Damage: " + baseRanged);

            baseElemental = elementalDamageComponent.ElementalMultiplier;
            Debug.Log("Base Elemental Damage: " + baseElemental);

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

            if(healthComponent.CurrentHealth <= 0){
                SceneManager.LoadScene("MainMenuScene");
            }

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
            float speedBonus = 0;
            float manaBonus = 0;
            float attackBonus = 0;
            float meleeBonus = 0;
            float rangedBonus = 0;
            float elementalBonus = 0;

            foreach (var item in inventoryComponent.GetItems())
            {
                if (item.itemData.type == Type.Passive)
                {
                    hpBonus += item.itemData.hpModifier * item.quantity;
                    speedBonus += item.itemData.moveSpeedModifier * item.quantity / 100;
                    manaBonus += item.itemData.manaModifier * item.quantity;
                    attackBonus += item.itemData.attackModifier * item.quantity;
                    meleeBonus += item.itemData.meleeDamageModifier * item.quantity;
                    rangedBonus += item.itemData.rangedDamageModifier * item.quantity;
                    elementalBonus += item.itemData.elementalDamageModifier * item.quantity;
                }
            }

            int newMaxHP = Mathf.RoundToInt(baseMaxHealth + hpBonus);
            int currentHP = healthComponent.CurrentHealth;
            int newMaxMana = Mathf.RoundToInt(baseMaxMana + manaBonus);
            int currentMana = manaComponent.CurrentMana;
            float newSpeed = baseSpeed + speedBonus;
            int newAttack = Mathf.RoundToInt(baseAttack + attackBonus);
            float newMelee = baseMelee + meleeBonus;
            float newRanged = baseRanged + rangedBonus;
            float newElemental = baseElemental + elementalBonus;

            healthComponent.MaximumHealth = newMaxHP;
            healthComponent.CurrentHealth = Mathf.Clamp(currentHP, 0, newMaxHP);
            manaComponent.MaximumMana = newMaxMana;
            manaComponent.CurrentMana = Mathf.Clamp(currentMana, 0, newMaxMana);
            movementComponent.MoveSpeed = newSpeed;
            attackComponent.BaseAttackDamage = newAttack;
            meleeDamageComponent.MeleeMultiplier = newMelee;
            rangedDamageComponent.RangedMultiplier = newRanged;
            elementalDamageComponent.ElementalMultiplier = newElemental;

            healthUI.UpdateUI();
            manaUI.UpdateUI();
            movementUI.UpdateUI();
            attackUI.UpdateUI();
            meleeDamageUI.UpdateUI();
            rangedDamageUI.UpdateUI();
            elementalDamageUI.UpdateUI();
        }
    }
}

