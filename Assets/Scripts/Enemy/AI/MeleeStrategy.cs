using Components.Movements;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.AI
{
    [CreateAssetMenu(fileName = "Melee Strategy", menuName = "Features/Strategy/Melee Strategy", order = 0)]
    public class MeleeStrategy : EnemyStrategy
    {
        public override void Execute(Enemy enemy, Transform target)
        {
            var navMeshAgent = enemy.GetComponent<NavMeshAgent>();
            if (navMeshAgent == null)
            {
                Debug.LogWarning("Navigation Mesh Agent is not set for " + enemy.name);
                return;
            }
            // Move towards the target
            navMeshAgent.Move(target.position);
        }
    }
}
