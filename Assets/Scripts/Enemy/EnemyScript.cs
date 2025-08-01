using Components.Health;
using Components.Movements;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections;
using Player;

namespace Enemy
{
    enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    public class EnemyScript : MonoBehaviour
    {
        [SerializeField]
        private IMovementComponent movementComponent;

        [SerializeField]
        private HealthComponent healthComponent;

        [SerializeField]
        private EnemyAttackComponent attackComponent;
        
        [SerializeField]
        private EnemyGoldDropComponent goldDropComponent;

        [CanBeNull]
        private GameObject target;

        private EnemyState currentState = EnemyState.Idle;

        [SerializeField]
        private int attackDamage = 3;
        
        [SerializeField]
        private float destroyDelay = 1.5f;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            movementComponent ??= GetComponent<IMovementComponent>();
            attackComponent ??= GetComponentInChildren<EnemyAttackComponent>();
            attackComponent.OnPlayerEntered += HandlePlayerEnteredAttackZone;
            attackComponent.OnPlayerLeft += HandlePlayerLeftAttackZone;
            healthComponent ??= GetComponent<HealthComponent>();
            goldDropComponent ??= GetComponent<EnemyGoldDropComponent>();
            
            // Subscribe to the OnDeath event
            if (healthComponent != null)
            {
                healthComponent.OnDeath += HandleDeath;
            }
        }
        
        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (healthComponent != null)
            {
                healthComponent.OnDeath -= HandleDeath;
            }
            
            if (attackComponent != null)
            {
                attackComponent.OnPlayerEntered -= HandlePlayerEnteredAttackZone;
                attackComponent.OnPlayerLeft -= HandlePlayerLeftAttackZone;
            }
        }

        void Start()
        {
            var room = transform.parent;
            if (room != null)
            {
                foreach (Transform child in room)
                {
                    if (child.CompareTag("Player"))
                    {
                        // Found the sibling with tag "Player"
                        Debug.Log("Found Player: " + child.name);
                        target = child.gameObject;
                        break;
                    }
                }
            }

            if (target != null)
            {
                currentState = EnemyState.Chasing;
            }
        }

        private void FixedUpdate()
        {
            // Don't process movement or attacks if dead
            if (currentState == EnemyState.Dead)
                return;
                
            switch (currentState)
            {
                case EnemyState.Chasing:
                    movementComponent.Move(target?.transform.position ?? Vector2.down);
                    break;
                case EnemyState.Attacking:
                    attackComponent.Attack(target: target?.gameObject, damage: attackDamage);
                    currentState = EnemyState.Chasing;
                    break;
            }
        }

        private void HandlePlayerEnteredAttackZone(GameObject player)
        {
            // Don't change state if dead
            if (currentState == EnemyState.Dead)
                return;
                
            currentState = EnemyState.Attacking;
        }

        private void HandlePlayerLeftAttackZone()
        {
            // Don't change state if dead
            if (currentState == EnemyState.Dead)
                return;
                
            currentState = EnemyState.Chasing;
        }
        
        private void HandleDeath()
        {
            // Change state to dead
            currentState = EnemyState.Dead;
            
            // Disable movement and collisions
            if (movementComponent != null)
            {
                var movementBehavior = movementComponent as MonoBehaviour;
                if (movementBehavior != null)
                {
                    movementBehavior.enabled = false;
                }
            }
            
            // Disable any colliders to prevent further interactions
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }
            
            // Drop gold when enemy dies
            if (goldDropComponent != null)
            {
                goldDropComponent.DropGold();
            }
            
            // Destroy the enemy after a short delay to allow for any death animations or effects
            StartCoroutine(DestroyAfterDelay());
        }
        
        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(destroyDelay);
            
            // Return the enemy to the pool if using object pooling, otherwise destroy it
            var poolComponent = FindFirstObjectByType<EnemySpawnerComponent>();
            if (poolComponent != null)
            {
                poolComponent.ReturnEnemyToPool(gameObject);
            }
            else
            {
                Destroy(gameObject);
            }
        }
    }
}
