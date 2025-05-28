using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Components.Movements
{
    /// <summary>
    /// Concrete implementation of the IMovementComponent interface is intended to be used for ground movement
    /// </summary>
    public class GroundMovementComponent : MonoBehaviour
    {
        [SerializeField]
        [Range(1.0f, 100.0f)]
        private float moveSpeed;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        public event Action<float> OnMovementSpeedChanged;

        [SerializeField]
        private Rigidbody2D rigidBody;

        private void Awake()
        {
            rigidBody ??= GetComponent<Rigidbody2D>();
            OnMovementSpeedChanged?.Invoke(moveSpeed);
        }

        public void Move(Vector2 direction)
        {
            rigidBody.linearVelocity = direction * moveSpeed;
        }
    }
}
