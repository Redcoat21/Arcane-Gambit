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
        
        [SerializeField]
        private int currentHealth;

        public int MaximumHealth
        {
            get => maximumHealth;
            set => maximumHealth = value;
        }
        
        public int CurrentHealth
        {
            get => currentHealth;
            set => currentHealth = value;
        }

        // OnHealthChanged is an event that will be triggered when the health of the object changes
        public event Action<int> OnHealthChanged;
        
        // OnDeath is an event that will be triggered when health reaches zero or below
        public event Action OnDeath;

        
        private void Awake()
        {
            currentHealth = maximumHealth;
        }
        
        /// <summary>
        /// Decrease the health of the entity by the damage amount
        /// </summary>
        /// <param name="damage">Amount of damage taken</param>
        public void TakeDamage(int damage)
        {
            currentHealth -= damage;
            OnHealthChanged?.Invoke(currentHealth);
            
            // Check if entity has died
            if (currentHealth <= 0)
            {
                // Invoke death event
                OnDeath?.Invoke();
            }
        }
        
        /// <summary>
        /// Increase the health of the entity by heal amount
        /// </summary>
        /// <param name="healAmount">Amount of health to heal</param>
        public void Heal(int healAmount)
        {
            int tempHealth = currentHealth + healAmount;
            currentHealth = tempHealth > maximumHealth ? maximumHealth : tempHealth;
            OnHealthChanged?.Invoke(currentHealth);
        }
    }
}
