using System;
using UnityEngine;

namespace Components.Movements
{
    public class GroundMovementComponent : MonoBehaviour
    {
        [SerializeField]
        [Range(1.0f, 100.0f)]
        private float moveSpeed = 5f;

        [SerializeField]
        private Rigidbody2D rigidBody;

        [SerializeField]
        private float dashMultiplier = 2f;

        [SerializeField]
        private float dashDuration = 0.5f;

        [SerializeField]
        private float dashCooldown = 1f;

        private bool isDashing = false;
        private bool canDash = true;
        private Vector2 lastDirection = Vector2.zero;

        [SerializeField] private DashCooldownUI dashUI;

        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        public event Action<float> OnMovementSpeedChanged;

        private void Awake()
        {
            rigidBody ??= GetComponent<Rigidbody2D>();
            OnMovementSpeedChanged?.Invoke(moveSpeed);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space) && canDash)
            {
                StartCoroutine(Dash());
            }
        }

        public void Move(Vector2 direction)
        {
            if (direction != Vector2.zero)
                lastDirection = direction;

            float currentSpeed = isDashing ? moveSpeed * dashMultiplier : moveSpeed;
            rigidBody.linearVelocity = direction * currentSpeed * 5f;
        }

        private System.Collections.IEnumerator Dash()
        {
            isDashing = true;
            canDash = false;
            yield return new WaitForSeconds(dashDuration);
            isDashing = false;
            dashUI?.StartCooldown(dashCooldown);
            yield return new WaitForSeconds(dashCooldown);
            canDash = true;
        }
    }
}
