using System;
using Components.Movements;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyMovementComponent : MonoBehaviour, IMovementComponent
    {
        private NavMeshAgent agent;

        private void Awake()
        {
            agent ??= GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        public void Move(Vector2 direction)
        {
            agent.SetDestination(direction);
        }
    }
}
