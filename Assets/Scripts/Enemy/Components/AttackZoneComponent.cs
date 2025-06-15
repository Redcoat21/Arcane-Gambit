using System;
using UnityEngine;

namespace Enemy.Components
{
    public class AttackZoneComponent : MonoBehaviour
    {
        public event Action<Collider2D> OnEnemyEnter;
        public event Action<Collider2D> OnEnemyExit;
        public void OnTriggerEnter2D(Collider2D other)
        {
            OnEnemyEnter?.Invoke(other);
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            OnEnemyExit?.Invoke(other);
        }
    }
}
