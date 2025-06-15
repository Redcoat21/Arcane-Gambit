using JetBrains.Annotations;
using UnityEngine;

namespace Enemy.AI
{
    public class EnemyAIComponent : MonoBehaviour
    {
        [SerializeField]
        [CanBeNull]
        private Transform target;

        [SerializeField]
        private EnemyStrategy strategy;

        [SerializeField]
        private Enemy enemy;

        public EnemyStrategy Strategy
        {
            get => strategy;
            set => strategy = value;
        }

        public Enemy Enemy
        {
            get => enemy;
            set => enemy = value;
        }

        private void FixedUpdate()
        {
            if (Strategy == null)
            {
                Debug.LogWarning("AI Strategy is not set for " + enemy.name);
                return;
            }

            if (target == null)
            {
                Debug.LogWarning("Target is not set for " + enemy.name);
                return;
            }

            strategy.Execute(enemy, target);
        }
    }
}
