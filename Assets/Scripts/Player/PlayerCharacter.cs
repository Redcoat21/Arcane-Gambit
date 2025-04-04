using System;
using Components.Health;
using Components.Movements;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player
{
    public class PlayerCharacter : MonoBehaviour
    {
        [SerializeField]
        private GroundMovementComponent movementComponent;
        [SerializeField]
        private HealthComponent healthComponent;
        
        private void Awake()
        {
            movementComponent ??= GetComponent<GroundMovementComponent>();
            healthComponent ??= GetComponent<HealthComponent>();
        }

        private void FixedUpdate()
        {
            var input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
            movementComponent?.Move(input);
        }
    }
}
