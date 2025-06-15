using Components.Health;
using UnityEngine;

namespace Components.Attack
{
    public class BasicAttackComponent : MonoBehaviour, IAttackComponent
    {
        [SerializeField]
        [Range(1, 100)]
        private int attackDamage = 5;
        
        public void Attack(GameObject target)
        {
            var healthComponent = target.GetComponent<HealthComponent>();
            healthComponent.TakeDamage(attackDamage);
        }
    }
}
