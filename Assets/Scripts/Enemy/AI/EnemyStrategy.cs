using UnityEngine;

namespace Enemy.AI
{
    public abstract class EnemyStrategy : ScriptableObject
    {
        public abstract void Execute(Enemy enemy, Transform target);
    }
}
