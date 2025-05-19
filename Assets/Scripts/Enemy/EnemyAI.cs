using DefaultNamespace;
using UnityEngine;
using UnityEngine.AI;

namespace Enemy
{
    public class EnemyAI : MonoBehaviour
    {
        [Header("Detection")]
        public float visionRange = 5f;
        public LayerMask obstacleMask;

        [Header("Navigation")]
        public float patrolSpeed = 2f;
        public float chaseSpeed = 4f;

        private Transform player;
        private NavMeshAgent agent;
        private RoomInstance currentRoom;
        private bool isChasing;

        void Awake()
        {
            agent = GetComponent<NavMeshAgent>();
            player = GameObject.FindGameObjectWithTag("Player").transform;
            currentRoom = GetComponentInParent<RoomInstance>();
        
            agent.speed = patrolSpeed;
            agent.updateRotation = false;
            agent.updateUpAxis = false;
        }

        void Update()
        {
            if (!IsInSameRoom() || player == null) return;

            if (CanSeePlayer())
            {
                ChasePlayer();
            }
            else if (isChasing)
            {
                StopChasing();
            }
        }

        bool CanSeePlayer()
        {
            // Distance check
            if (Vector2.Distance(transform.position, player.position) > visionRange)
                return false;

            // Line of sight check
            RaycastHit2D hit = Physics2D.Raycast(
                transform.position,
                player.position - transform.position,
                visionRange,
                obstacleMask
            );

            return hit.collider == null;
        }

        void ChasePlayer()
        {
            isChasing = true;
            agent.speed = chaseSpeed;
            agent.SetDestination(player.position);
        }

        void StopChasing()
        {
            isChasing = false;
            agent.speed = patrolSpeed;
            agent.ResetPath();
        }

        bool IsInSameRoom()
        {
            return currentRoom != null && 
                   currentRoom.IsPositionInsideRoom(player.position);
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, visionRange);
        }
        
    }
}
