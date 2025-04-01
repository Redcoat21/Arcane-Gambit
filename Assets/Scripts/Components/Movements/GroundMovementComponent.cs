using System;
using UnityEngine;

namespace Components.Movements
{
    /// <summary>
    /// Concrete implementation of the IMovementComponent interface is intended to be used for ground movement
    /// </summary>
    public class GroundMovementComponent : MonoBehaviour, IMovementComponent
    {
        [SerializeField]
        [Range(1.0f, 100.0f)]
        private float moveSpeed;

        private Rigidbody2D _rigidbody;

        private void Awake()
        {
            _rigidbody = GetComponent<Rigidbody2D>();
        }

        public void Move(Vector2 direction)
        {
            _rigidbody.linearVelocity = direction * moveSpeed;
        }
    }
}
