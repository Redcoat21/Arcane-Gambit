using System;
using UnityEngine;
using Components.MeleeDamage;
using Components.RangedDamage;
using Components.ElementalDamage;

namespace Components.Attack
{
    public enum DamageType { Melee, Ranged, Elemental }
    /// <summary>
    /// Component that handles attack damage and attack events for an entity.
    /// </summary>
    public class AttackComponent : MonoBehaviour
    {
        [SerializeField] private DamageType damageType;

        [Header("Optional Damage Sources")]
        [SerializeField] private MeleeDamageComponent meleeDamageComponent;
        [SerializeField] private RangedDamageComponent rangedDamageComponent;
        [SerializeField] private ElementalDamageComponent elementalDamageComponent;
        [SerializeField] private GameObject rotatingWeaponPrefab;
        [SerializeField] private WeaponData currentWeapon;
        [SerializeField] private GameObject spellProjectilePrefab;
        [SerializeField] private SpellData currentSpell;

        public WeaponData CurrentWeapon
        {
            get => currentWeapon;
            set => currentWeapon = value;
        }

        [SerializeField]
        [Range(1, 1000)]
        private int baseAttackDamage = 100;

        [SerializeField]
        private float attackCooldown = 1f;

        private float lastAttackTime;
        private float lastSpellCastTime;

        public int BaseAttackDamage
        {
            get => baseAttackDamage;
            set => baseAttackDamage = value;
        }

        public SpellData CurrentSpell
        {
            get => currentSpell;
            set => currentSpell = value;
        }

        public DamageType CurrentDamageType
        {
            get => damageType;
            set => damageType = value;
        }

        public float AttackCooldown
        {
            get => attackCooldown;
            set => attackCooldown = Mathf.Max(0f, value);
        }

        public void SetDamageType(DamageType newType)
        {
            damageType = newType;
            Debug.Log("Damage type set to: " + damageType);
        }

        // Triggered when an attack happens
        public event Action<int> OnAttackPerformed;
        public event Action<int> OnAttackChanged;

        /// <summary>
        /// Attempt to perform an attack if cooldown allows.
        /// </summary>
        public void TryAttack()
        {
            if (Time.time >= lastAttackTime + attackCooldown)
            {
                PerformAttack();
            }
        }

        private void SpawnRotatingWeapon()
        {
            if (rotatingWeaponPrefab == null)
            {
                Debug.LogWarning("Rotating weapon prefab is not assigned!");
                return;
            }

            if (currentWeapon == null)
            {
                Debug.LogWarning("Current weapon is not assigned!");
                return;
            }

            if (currentWeapon.weaponSprite == null)
            {
                Debug.LogWarning($"Weapon '{currentWeapon.weaponName}' has no sprite assigned!");
                return;
            }

            GameObject instance = Instantiate(rotatingWeaponPrefab, transform.position + Vector3.right * 1.5f, Quaternion.identity);

            SpriteRenderer sr = instance.GetComponent<SpriteRenderer>();
            if (sr != null)
            {
                sr.sprite = currentWeapon.weaponSprite;
                sr.enabled = true;
                sr.sortingOrder = 10;
                Debug.Log("Sprite assigned at runtime: " + sr.sprite.name);
            }
            else
            {
                Debug.LogWarning("SpriteRenderer not found on instantiated weapon prefab.");
            }

            WeaponRotator rotator = instance.GetComponent<WeaponRotator>();
            if (rotator != null)
            {
                rotator.pivot = this.transform;
                rotator.duration = currentWeapon != null ? currentWeapon.attackSpeed : attackCooldown;
                rotator.rotationSpeed = 360f / currentWeapon.attackSpeed;
            }
            else
            {
                Debug.LogWarning("WeaponRotator component not found on instantiated prefab.");
            }
        }

        private void TryCastSpell()
        {
            var manaComponent = GetComponent<Mana.ManaComponent>();
            if (manaComponent == null || manaComponent.CurrentMana < currentSpell.manaCost)
            {
                Debug.Log("Not enough mana!");
                return;
            }

            CastSpell();
            lastSpellCastTime = Time.time;
            manaComponent.ReduceMana(currentSpell.manaCost); // Assume this exists
            SpellUI.Instance.TriggerCooldown();
        }

        private void CastSpell()
        {
            if (spellProjectilePrefab == null)
            {
                Debug.LogWarning("Spell projectile prefab not assigned!");
                return;
            }

            // Get world position of the mouse
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePos.z = 0f;

            // Direction from player to mouse
            Vector3 direction = (mousePos - transform.position).normalized;

            // Spawn slightly offset toward the direction (so it doesn't push player)
            Vector3 spawnPos = transform.position + direction * 1.5f;

            // Instantiate the projectile
            GameObject projectile = Instantiate(spellProjectilePrefab, spawnPos, Quaternion.identity);

            // Assign spell sprite
            SpriteRenderer sr = projectile.GetComponent<SpriteRenderer>();
            if (sr != null && currentSpell.spellSprite != null)
                sr.sprite = currentSpell.spellSprite;

            // Assign velocity to Rigidbody2D
            Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                Vector2 dir = (mousePos - spawnPos).normalized;
                rb.linearVelocity = dir * 10f;

                // OPTIONAL: Ignore collision with the player
                Collider2D playerCollider = GetComponent<Collider2D>();
                Collider2D projectileCollider = projectile.GetComponent<Collider2D>();
                if (playerCollider != null && projectileCollider != null)
                {
                    Physics2D.IgnoreCollision(projectileCollider, playerCollider);
                }
            }

            // Auto-destroy after 1 second
            Destroy(projectile, 1f);

            Debug.Log($"Spell cast: {currentSpell.spellName}");
        }

        /// <summary>
        /// Perform the attack and trigger event.
        /// </summary>
        private void PerformAttack()
        {
            lastAttackTime = Time.time;
            int totalDamage = CalculateDamage();

            SpawnRotatingWeapon();

            // Optionally: play animation, sound, or spawn projectile here

            Debug.Log("Attack performed with damage: " + totalDamage);
            OnAttackPerformed?.Invoke(totalDamage);
            OnAttackChanged?.Invoke(baseAttackDamage);
        }

        private int CalculateDamage()
        {
            meleeDamageComponent ??= GetComponent<MeleeDamageComponent>();
            rangedDamageComponent ??= GetComponent<RangedDamageComponent>();
            elementalDamageComponent ??= GetComponent<ElementalDamageComponent>();
            float multiplier = 0f;
            switch (damageType)
            {
                case DamageType.Melee:
                    multiplier = meleeDamageComponent != null ? meleeDamageComponent.MeleeMultiplier : 0f;
                    break;
                case DamageType.Ranged:
                    multiplier = rangedDamageComponent != null ? rangedDamageComponent.RangedMultiplier : 0f;
                    break;
                case DamageType.Elemental:
                    multiplier = elementalDamageComponent != null ? elementalDamageComponent.ElementalMultiplier : 0f;
                    break;
            }
            
            return Mathf.RoundToInt(baseAttackDamage + baseAttackDamage * (multiplier/100));
        }

        private void Update()
        {
            if (Input.GetMouseButtonDown(0)) // left mouse click
            {
                if(currentWeapon.isMelee){
                    CurrentDamageType = DamageType.Melee;
                }
                else{
                    CurrentDamageType = DamageType.Ranged;
                }
                TryAttack();
            }
            if (Input.GetKeyDown(KeyCode.Alpha1)) // top row number 1 key
            {
                CurrentDamageType = DamageType.Melee;
            }
            if (Input.GetKeyDown(KeyCode.Alpha2)) // for top row number 2 key
            {
                CurrentDamageType = DamageType.Ranged;
            }
            if (Input.GetKeyDown(KeyCode.Alpha3)) // for top row number 3 key
            {
                CurrentDamageType = DamageType.Elemental;
            }

            if (Input.GetKeyDown(KeyCode.F)) // cast spell
            {
                Debug.Log("spell test");
                CurrentDamageType = DamageType.Elemental;
                TryCastSpell();
            }
        }
    }
}