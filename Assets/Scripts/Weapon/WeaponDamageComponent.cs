using UnityEngine;

namespace Enemy
{
    public class WeaponDamageComponent : MonoBehaviour
    {
        [SerializeField] private int damage = 1;
        
        // Optional critical hit chance and multiplier
        [SerializeField] private float criticalHitChance = 0.1f;
        [SerializeField] private float criticalHitMultiplier = 2f;
        
        // Returns the damage value, potentially modified by critical hits
        public int GetDamage()
        {
            // Check for critical hit
            if (Random.value < criticalHitChance)
            {
                return Mathf.RoundToInt(damage * criticalHitMultiplier);
            }
            
            return damage;
        }
        
        // Allow setting damage from other scripts
        public void SetDamage(int newDamage)
        {
            damage = newDamage;
        }
    }
}
