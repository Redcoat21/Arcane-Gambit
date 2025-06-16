using System;
using System.Linq;
using Components.Health;
using Components.Movements;
using Enemy.Components;
using UnityEngine;

namespace Enemy
{
    public class Enemy : MonoBehaviour
    {
        [Header("Movement Components")]
        [SerializeField]
        private GroundMovementComponent movementComponent;

        [SerializeField]
        private EnemyAIComponent enemyAIComponent;

        [Header("Health")]
        [SerializeField]
        private HealthComponent healthComponent;

        public void Awake()
        {
            movementComponent ??= GetComponent<GroundMovementComponent>();
            enemyAIComponent ??= GetComponent<EnemyAIComponent>();
            healthComponent ??= GetComponent<HealthComponent>();
        }

        public void Update()
        {
            var nextDestination = enemyAIComponent.GetNextDestination();
            if (nextDestination != null)
            {
                movementComponent.Move(new Vector2(nextDestination.Value.x, nextDestination.Value.y));
            }
        }

        public void OnEnable()
        {
            var player = GameObject.FindGameObjectsWithTag("Player").First();
            enemyAIComponent.Target = player.transform;
        }
    }
}
