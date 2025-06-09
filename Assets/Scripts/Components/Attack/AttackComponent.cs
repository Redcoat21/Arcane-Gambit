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

        /// <summary>
        /// Perform the attack and trigger event.
        /// </summary>
        private void PerformAttack()
        {
            lastAttackTime = Time.time;
            int totalDamage = CalculateDamage();

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
    }
}