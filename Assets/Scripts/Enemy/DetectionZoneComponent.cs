using System;
using UnityEngine;

namespace Enemy
{
    [RequireComponent(typeof(Collider2D))]
    public class DetectionZoneComponent : MonoBehaviour
    {
        private Collider2D detectionZone;

        public event Action<GameObject, Vector3> OnPlayerDetected;

        private void Awake()
        {
            detectionZone ??= GetComponent<Collider2D>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                OnPlayerDetected?.Invoke(other.gameObject, other.gameObject.transform.position);
            }
        }
    }
}
