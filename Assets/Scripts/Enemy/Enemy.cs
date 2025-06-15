using Components.Health;
using UnityEngine;
using Components.Attack;
using Enemy.AI;
using Enemy.Components;
using UnityEngine.Serialization;
using Utility;

namespace Enemy
{
    enum EnemyState
    {
        Idle,
        Chasing,
        Attacking,
        Dead
    }

    public class Enemy : MonoBehaviour
    {
        [Header("Base Functionality")]
        [SerializeField]
        private HealthComponent healthComponent;

        [SerializeField]
        private BasicAttackComponent attackComponent;

        [SerializeField]
        private AttackZoneComponent attackZoneComponent;

        [SerializeField]
        private EnemyGoldDropComponent goldDropComponent;

        [Header("AI Functionality")]
        [SerializeField]
        private EnemyAIComponent enemyAIComponent;

        [SerializeField]
        private float destroyDelay = 1.5f;

        [SerializeField]
        private MeleeStrategy attackStrategy;

        private EnemyState currentState = EnemyState.Idle;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            attackComponent ??= GetComponentInChildren<BasicAttackComponent>();
            goldDropComponent ??= GetComponent<EnemyGoldDropComponent>();

            ComponentUtilities.SafeAssign(healthComponent, h => h.OnDeath += HandleDeath, nameof(HealthComponent),
                gameObject);
            ComponentUtilities.SafeAssign(attackZoneComponent, a =>
            {
                a.OnEnemyEnter += HandlePlayerEnteredAttackZone;
                a.OnEnemyExit += HandlePlayerLeftAttackZone;
            }, nameof(AttackZoneComponent), gameObject);
            ComponentUtilities.SafeAssign(enemyAIComponent, e =>
                {
                    e.Strategy = attackStrategy;
                    e.Enemy = this;
                }, nameof(EnemyAIComponent),
                gameObject);
        }

        private void OnDestroy()
        {
            // Unsubscribe to prevent memory leaks
            if (healthComponent != null)
            {
                healthComponent.OnDeath -= HandleDeath;
            }
        }

        private void OnEnable()
        {
            // On enabling the enemy, assign the player as the target and chase the player
            // Re-enabling mean the enemy is spawned from the object pool
            currentState = EnemyState.Chasing;
        }

        private void HandlePlayerEnteredAttackZone(Collider2D other)
        {
            // Don't change state if dead
            if (currentState == EnemyState.Dead)
                return;

            if (other.CompareTag("Player"))
            {
                currentState = EnemyState.Attacking;
            }
        }

        private void HandlePlayerLeftAttackZone(Collider2D other)
        {
            // Don't change state if dead
            if (currentState == EnemyState.Dead)
                return;

            if (other.CompareTag("Player"))
            {
                currentState = EnemyState.Chasing;
            }
        }

        private void HandleDeath()
        {
            // Change state to dead
            currentState = EnemyState.Dead;

            // Disable any colliders to prevent further interactions
            var colliders = GetComponentsInChildren<Collider2D>();
            foreach (var collider in colliders)
            {
                collider.enabled = false;
            }

            // Drop gold when the enemy dies
            if (goldDropComponent != null)
            {
                goldDropComponent.DropGold();
            }
        }
    }
}
