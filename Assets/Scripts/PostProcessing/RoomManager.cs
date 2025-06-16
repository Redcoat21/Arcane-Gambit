using Edgar.Unity;
using Levels.Room;
using UnityEngine;

namespace PostProcessing
{
    public class RoomManager : MonoBehaviour
    {
        public RoomInstanceGrid2D RoomInstance { get; set; }
        public CombatRoom CombatRoomComponent { get; set; }

        public void OnRoomEnter(GameObject otherColliderGameObject)
        {
            if (CombatRoomComponent != null)
            {
                Debug.Log(
                    $"Spawning {CombatRoomComponent.WaveNumber} waves in room {RoomInstance.RoomTemplateInstance.name}");
            }
            else
            {
                Debug.Log($"Room enter {RoomInstance}");
            }
        }

        public void OnRoomLeave(GameObject otherColliderGameObject)
        {
            Debug.Log($"Room leave {RoomInstance.Room.GetDisplayName()}");
            // Handle any cleanup when leaving the room
        }
    }
}
