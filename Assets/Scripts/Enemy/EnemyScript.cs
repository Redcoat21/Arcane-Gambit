using Components.Health;
using Components.Movements;
using JetBrains.Annotations;
using UnityEngine;
using System.Collections;
using System.Linq;
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
        [SerializeField]
        private Transform target;

        private EnemyState currentState = EnemyState.Idle;

        [SerializeField]
        private int attackDamage = 3;
        
        [SerializeField]
        private float destroyDelay = 1.5f;
        
        // Reference to the pool component that spawned this enemy
        private EnemySpawnerComponent poolComponent;

        // Method to set the pool component reference
        public void SetPoolComponent(EnemySpawnerComponent component)
        {
            poolComponent = component;
        }

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
            if (target != null)
            {
                currentState = EnemyState.Chasing;
            }
            else
            {
                Debug.LogWarning("No target found");
            }
        }

        private void FixedUpdate()
        {

            if (target == null)
            {
                var spawnRoom = Object.FindObjectsByType<GameObject>(FindObjectsSortMode.InstanceID)
                    .FirstOrDefault(go => go.name.Contains("Spawn - "));
                
                if (spawnRoom != null)
                {
                    Debug.Log("Found SpawnRoom: " + spawnRoom.name);
                    foreach (Transform child in spawnRoom.transform)
                    {
                        Debug.Log(child.name);
                        if (child.CompareTag("Player"))
                        {
                            // Found the sibling with tag "Player"
                            Debug.Log("Found Player: " + child.name);
                            target = child.gameObject.transform;
                            break;
                        }
                    }
                }
                else
                {
                    Debug.LogWarning("Could not find any GameObject containing 'SpawnRoom' in its name");
                }
            }
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

            var player = target;
            player?.GetComponent<CurrencyComponent>()?.AddGold(5);
            
            // Destroy the enemy after a short delay to allow for any death animations or effects
            StartCoroutine(DestroyAfterDelay());
        }
        
        private IEnumerator DestroyAfterDelay()
        {
            yield return new WaitForSeconds(destroyDelay);
            
            // Return the enemy to the pool if using object pooling, otherwise destroy it
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
