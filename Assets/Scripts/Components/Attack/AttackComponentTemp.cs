using System;
using UnityEngine;
using Components.MeleeDamage;
using Components.RangedDamage;
using Components.ElementalDamage;
using Components.Health;

namespace Components.Attack
{
    public enum DamageType { Melee, Ranged, Elemental }
    /// <summary>
    /// Component that handles attack damage and attack events for an entity.
    /// </summary>
    public class AttackComponentTemp : MonoBehaviour, IAttackComponent
    {
        [SerializeField] private DamageType damageType;

        [Header("Optional Damage Sources")]
        [SerializeField] private MeleeDamageComponent meleeDamageComponent;
        [SerializeField] private RangedDamageComponent rangedDamageComponent;
        [SerializeField] private ElementalDamageComponent elementalDamageComponent;
        [SerializeField] private GameObject rotatingWeaponPrefab;
        [SerializeField] private WeaponData currentWeapon;
        private int damage = 5;
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

        public int BaseAttackDamage
        {
            get => baseAttackDamage;
            set => baseAttackDamage = value;
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
        }

        public void Attack(GameObject target)
        {
            var healthComponent = target.GetComponent<HealthComponent>();
            if (!healthComponent)
            {
                Debug.LogWarning($"Target {target.name} does not have a HealthComponent!");
                return;
            }
            healthComponent.TakeDamage(damage);
        }
    }
}