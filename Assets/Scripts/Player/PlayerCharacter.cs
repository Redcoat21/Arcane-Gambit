using System;
using Components.Health;
using Components.Movements;
using UnityEngine;

namespace Player
{
    public class PlayerCharacter : MonoBehaviour
    {
        private GroundMovementComponent _movementComponent;
        private HealthComponent _healthComponent;
        
        private void Awake()
        {
            _movementComponent = GetComponent<GroundMovementComponent>();
            _healthComponent = GetComponent<HealthComponent>();
        }

        private void FixedUpdate()
        {
            var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            _movementComponent?.Move(input);
        }
    }
}
