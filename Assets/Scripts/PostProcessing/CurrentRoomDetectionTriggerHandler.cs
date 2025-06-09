using DefaultNamespace;
using UnityEngine;

namespace PostProcessing
{
    public class CurrentRoomDetectionTriggerHandler : MonoBehaviour
    {
        private RoomManager roomManager;

        public void Start()
        {
            roomManager = transform.parent.parent.gameObject.GetComponent<RoomManager>();
        }

        public void OnTriggerEnter2D(Collider2D otherCollider)
        {
            if (otherCollider.gameObject.CompareTag("Player"))
            {
                roomManager?.OnRoomEnter(otherCollider.gameObject);
            }
        }

        public void OnTriggerExit2D(Collider2D otherCollider)
        {
            if (otherCollider.gameObject.CompareTag("Player"))
            {
                roomManager?.OnRoomLeave(otherCollider.gameObject);
            }
        }
    }
}
