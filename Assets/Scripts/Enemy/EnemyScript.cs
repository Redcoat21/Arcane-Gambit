using Components.Health;
using Components.Movements;
using JetBrains.Annotations;
using UnityEngine;

namespace Enemy
{
    enum EnemyState
    {
        Idle,
        Chasing,
        Attacking
    }

    public class EnemyScript : MonoBehaviour
    {
        [SerializeField]
        private IMovementComponent movementComponent;

        [SerializeField]
        private EnemyAttackComponent attackComponent;

        [CanBeNull]
        private GameObject target;

        private EnemyState currentState = EnemyState.Idle;

        private int attackDamage = 3;

        public int AttackDamage
        {
            get => attackDamage;
            set => attackDamage = value;
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            movementComponent ??= GetComponent<IMovementComponent>();
            attackComponent ??= GetComponentInChildren<EnemyAttackComponent>();
            attackComponent.OnPlayerEntered += HandlePlayerEnteredAttackZone;
            attackComponent.OnPlayerLeft += HandlePlayerLeftAttackZone;
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

        // Update is called once per frame
        void Update()
        {
        }

        private void FixedUpdate()
        {
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
            currentState = EnemyState.Attacking;
        }

        private void HandlePlayerLeftAttackZone()
        {
            currentState = EnemyState.Chasing;
        }
    }
}
