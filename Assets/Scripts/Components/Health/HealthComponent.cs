using System;
using UnityEngine;

namespace Components.Health
{
    /// <summary>
    /// Component that handles the health of an entity
    /// </summary>
    public class HealthComponent : MonoBehaviour
    {
        [SerializeField]
        [Range(1, 100)]
        private int maximumHealth;
        
        public int MaximumHealth
        {
            get => maximumHealth;
            set => maximumHealth = value;
        }
        
        public int CurrentHealth { get; private set; }
        
        // OnHealthChanged is an event that will be triggered when the health of the object changes
        public event Action<int> OnHealthChanged;

        /// <summary>
        /// Decrease the health of the entity by the damage amount
        /// </summary>
        /// <param name="damage">Amount of damage taken</param>
        public void TakeDamage(int damage)
        {
            CurrentHealth -= damage;
            OnHealthChanged?.Invoke(CurrentHealth);
        }
        
        /// <summary>
        /// Increase the health of the entity by heal amount
        /// </summary>
        /// <param name="healAmount">Amount of health to heal</param>
        public void Heal(int healAmount)
        {
            int tempHealth = CurrentHealth + healAmount;
            CurrentHealth = tempHealth > maximumHealth ? maximumHealth : tempHealth;
            OnHealthChanged?.Invoke(CurrentHealth);
        }
    }
}
