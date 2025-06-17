using System;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy.Components
{
    [RequireComponent(typeof(NavMeshAgent))]
    public class EnemyAIComponent : MonoBehaviour
    {
        [SerializeField]
        private NavMeshAgent agent;

        [SerializeField]
        [CanBeNull]
        private Transform target;

        public Transform Target
        {
            get => target;
            set => target = value;
        }

        public void Awake()
        {
            agent ??= GetComponent<NavMeshAgent>();
            agent.updateRotation = false;
        }

        public Vector3? GetNextDestination()
        {
            if (target == null)
            {
                return null;
            }

            NavMeshPath path = new NavMeshPath();
            if (agent.CalculatePath(target.position, path))
            {
                if (path.corners.Length > 1)
                {
                    Vector3 nextPos = path.corners[1];
                    return nextPos;
                }
                else if (path.corners.Length == 1)
                {
                    // If there's only one corner, return the target position
                    return target.position;
                }
            }
            else
            {
                Debug.LogWarning("Failed to calculate path to target.");
            }

            return null;
        }

        public void Move()
        {
            if (target != null)
            {
                agent.SetDestination(target.position);
            }
            else 
            {
                Debug.LogWarning("Target is not set. Cannot move.");
            }
        }
    }
}
