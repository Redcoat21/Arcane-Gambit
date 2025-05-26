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
        private DetectionZoneComponent detectionZoneComponent;

        [CanBeNull]
        private Transform target;

        private EnemyState currentState = EnemyState.Idle;

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        private void Awake()
        {
            movementComponent ??= GetComponent<IMovementComponent>();
            detectionZoneComponent ??= GetComponentInChildren<DetectionZoneComponent>();

            if (detectionZoneComponent != null)
            {
                detectionZoneComponent.OnPlayerDetected += HandlePlayerDetected;
            }
        }

        void Start()
        {
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
                    movementComponent.Move(target?.position ?? Vector2.down);
                    break;
            }
        }

        private void HandlePlayerDetected(GameObject player, Vector3 position)
        {
            Debug.Log($"Player detected at position: {position}");
            target = player.transform;
            currentState = EnemyState.Chasing;
        }
    }
}
