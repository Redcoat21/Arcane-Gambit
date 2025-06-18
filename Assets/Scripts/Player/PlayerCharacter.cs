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
        [SerializeField] private SpellData equippedSpell;

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
            yield return new WaitForSeconds(0.05f);
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
            Debug.Log("PlayerManager found.");

                // Load inventory if PlayerManager has data
                if (PlayerManager.InventoryItems.Count > 0)
                {
                    Debug.Log($"Loading {PlayerManager.InventoryItems.Count} item(s) from PlayerManager.");
                    foreach (var item in PlayerManager.InventoryItems)
                    {
                        inventoryComponent.AddItem(item.itemData, item.quantity);
                        Debug.Log($"Added item from PlayerManager: {item.itemData.itemName} x{item.quantity}");
                    }
                    // inventoryComponent.LoadFromPlayerManager();
                }
                else
                {
                    Debug.Log("PlayerManager inventory is empty.");
                }

                // Equip weapon from manager if available
                if (PlayerManager.Weapon1 != null)
                {
                    weapon1 = PlayerManager.Weapon1;
                    
                    weapon1ImageUI.sprite = weapon1.weaponSprite;
                    weapon1InventoryImageUI.sprite = weapon1.weaponSprite;
                    attackComponent.CurrentWeapon = weapon1;
                    Debug.Log($"Loaded weapon1 from PlayerManager: {weapon1.weaponName}");
                }
                else
                {
                    Debug.Log("No weapon1 found in PlayerManager.");
                }

                // Load spell
                if (PlayerManager.EquippedSpell != null)
                {
                    EquipSpell(PlayerManager.EquippedSpell);
                    Debug.Log($"Loaded spell from PlayerManager: {equippedSpell.spellName}");
                }
                else
                {
                    EquipSpell(equippedSpell);
                    Debug.Log("No equipped spell found in PlayerManager.");
                }

                // Optional: Consumable
                if (PlayerManager.Consumable != null)
                {
                    consumable = PlayerManager.Consumable;
                    Debug.Log($"Loaded consumable from PlayerManager: {consumable.itemName}");
                }
                else
                {
                    Debug.Log("No consumable found in PlayerManager.");
                }
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
                
                attackComponent.CurrentWeapon = weapon1;
                PlayerManager.Weapon1 = weapon1;
            }

            Debug.Log("== Inventory Contents at Start ==");
            foreach (var item in inventoryComponent.GetItems())
            {
                Debug.Log($"{item.itemData.itemName} x{item.quantity}");
            }

            ApplyInventoryModifiers();
        }

        public void EquipSpell(SpellData spell)
        {
            equippedSpell = spell;
            attackComponent.CurrentSpell = equippedSpell;

            // Update the UI
            SpellUI.Instance?.UpdateSpellUI(equippedSpell);
        }

        public SpellData GetEquippedSpell()
        {
            return equippedSpell;
        }

        private void FixedUpdate()
        {
            Vector2 input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
            movementComponent?.Move(input);

            if(healthComponent.CurrentHealth <= 0){
                LevelManager.LevelCounter = 1;
                PlayerManager.Reset();
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

            // Store previous max values
            int previousMaxHP = healthComponent.MaximumHealth;
            int previousMaxMana = manaComponent.MaximumMana;

            // Calculate new max values
            int newMaxHP = Mathf.RoundToInt(baseMaxHealth + hpBonus);
            int newMaxMana = Mathf.RoundToInt(baseMaxMana + manaBonus);

            // Calculate how much to heal/add to current HP/Mana
            int hpGain = newMaxHP - previousMaxHP;
            int manaGain = newMaxMana - previousMaxMana;

            int currentHP = healthComponent.CurrentHealth;
            int currentMana = manaComponent.CurrentMana;
            float newSpeed = baseSpeed + speedBonus;
            int newAttack = Mathf.RoundToInt(baseAttack + attackBonus);
            float newMelee = baseMelee + meleeBonus;
            float newRanged = baseRanged + rangedBonus;
            float newElemental = baseElemental + elementalBonus;

            healthComponent.MaximumHealth = newMaxHP;
            healthComponent.CurrentHealth = Mathf.Clamp(healthComponent.CurrentHealth + hpGain, 0, newMaxHP);
            manaComponent.MaximumMana = newMaxMana;
            manaComponent.CurrentMana = Mathf.Clamp(manaComponent.CurrentMana + manaGain, 0, newMaxMana);
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

            PlayerManager.SetHealth(healthComponent.CurrentHealth, healthComponent.MaximumHealth);
            PlayerManager.SetMana(manaComponent.CurrentMana, manaComponent.MaximumMana);
            PlayerManager.MoveSpeed = movementComponent.MoveSpeed;
            PlayerManager.BaseAttack = attackComponent.BaseAttackDamage;
            PlayerManager.MeleeMultiplier = meleeDamageComponent.MeleeMultiplier;
            PlayerManager.RangedMultiplier = rangedDamageComponent.RangedMultiplier;
            PlayerManager.ElementalMultiplier = elementalDamageComponent.ElementalMultiplier;
            PlayerManager.UpdateInventory(inventoryComponent.GetItems());
            // PlayerManager.EquippedSpell = equippedSpell;
            // PlayerManager.Weapon1 = weapon1;
        }

        public void BuyWeapon(WeaponData newWeapon)
        {
            if (newWeapon == null) return;

            weapon1 = newWeapon;

            // Apply to attack component
            if (attackComponent != null)
                attackComponent.CurrentWeapon = weapon1;

            // Update UI
            if (weapon1ImageUI != null)
                weapon1ImageUI.sprite = weapon1.weaponSprite;
            if (weapon1InventoryImageUI != null)
                weapon1InventoryImageUI.sprite = weapon1.weaponSprite;

            // Update PlayerManager
            PlayerManager.Weapon1 = weapon1;

            Debug.Log($"Weapon bought: {weapon1.weaponName}");
        }

        public void BuySpell(SpellData newSpell)
        {
            if (newSpell == null) return;

            EquipSpell(newSpell);

            // Update PlayerManager
            PlayerManager.EquippedSpell = newSpell;

            Debug.Log($"Spell bought: {newSpell.spellName}");
        }

        public void BuyPotion()
        {
            healthUI.UpdateUI();
            manaUI.UpdateUI();
        }
    }
}