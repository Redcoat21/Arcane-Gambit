using System;
using Components.Health;
using UnityEngine;

namespace Enemy
{
    public class EnemyAttackComponent : MonoBehaviour, IAttackComponent
    {
        public Action<GameObject> OnPlayerEntered;
        public Action OnPlayerLeft;
        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerEntered?.Invoke(other.gameObject);
            }
        }

        private void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerLeft?.Invoke();
            }
        }
        
        public void Attack(int damage, GameObject target)
        {
            var healthComponent = target.gameObject.GetComponent<HealthComponent>();
            if (!healthComponent)
            {
                Debug.LogWarning("Target is invulnerable or does not have a HealthComponent.");
                return;
            }
            healthComponent.TakeDamage(damage*LevelManager.LevelCounter);
        }
    }
}
