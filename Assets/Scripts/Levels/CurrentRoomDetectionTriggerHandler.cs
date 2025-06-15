using UnityEngine;

namespace Levels
{
    public class CurrentRoomDetectionTriggerHandler : MonoBehaviour
    {
        private RoomManager roomManager;

        private void Awake()
        {
            roomManager = transform.parent.parent.gameObject.GetComponent<RoomManager>();
        }

        public void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                roomManager.OnRoomEnter(other.gameObject);
            }
        }

        public void OnTriggerExit2D(Collider2D other)
        {
            if (other.CompareTag("Player"))
            {
                roomManager.OnRoomLeave(other.gameObject);
            }
        }
    }
}
