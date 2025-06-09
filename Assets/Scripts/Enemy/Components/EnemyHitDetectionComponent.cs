using System;
using Components.Health;
using UnityEngine;

namespace Enemy.Components
{
    public class EnemyHitDetectionComponent : MonoBehaviour
    {
        // Event that fires when the enemy is hit
        public Action<int> OnEnemyHit;
        
        // Reference to the enemy's health component
        private HealthComponent healthComponent;

        private void Awake()
        {
            // Get reference to health component
            healthComponent = GetComponentInParent<HealthComponent>();
            
            if (healthComponent == null)
            {
                Debug.LogWarning("No HealthComponent found in parent. Enemy won't take damage.");
            }
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            // Check if the colliding object is a player weapon
            if (other.CompareTag("PlayerWeapon") || other.CompareTag("PlayerProjectile"))
            {
                // Try to get the damage component from the weapon
                WeaponDamageComponent damageComponent = other.GetComponent<WeaponDamageComponent>();
                
                if (damageComponent != null)
                {
                    int damage = damageComponent.GetDamage();
                    ApplyDamage(damage);
                }
                else
                {
                    // If no damage component found, apply default damage
                    ApplyDamage(1);
                }
                
                // If it's a projectile, you might want to destroy it
                if (other.CompareTag("PlayerProjectile"))
                {
                    Destroy(other.gameObject);
                }
            }
        }
        
        // Method to apply damage to the enemy
        public void ApplyDamage(int damage)
        {
            // Invoke the hit event
            OnEnemyHit?.Invoke(damage);
            
            // Apply damage to the health component if it exists
            if (healthComponent != null)
            {
                healthComponent.TakeDamage(damage);
            }
            else
            {
                Debug.LogWarning("Tried to apply damage but no HealthComponent found");
            }
        }
    }
}
