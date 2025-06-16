using System.Linq;
using Edgar.Unity;
using Edgar.Unity.Examples.CurrentRoomDetection;
using Levels.Room;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace PostProcessing
{
    [CreateAssetMenu(fileName = "CurrentRoomDetectionPostProcessing", menuName = "Features/Current Room Detection Processing", order = 0)]
    public class CurrentRoomDetectionPostProcessing : DungeonGeneratorPostProcessingGrid2D
    {
        public override void Run(DungeonGeneratorLevelGrid2D level)
        {
            foreach (var roomInstance in level.RoomInstances)
            {
                var roomTemplateInstance = roomInstance.RoomTemplateInstance;

                // Find floor tilemap layer
                var tilemaps = roomTemplateInstance.GetComponentsInChildren<Tilemap>();
                var floorTilemap = tilemaps.FirstOrDefault(x => x.gameObject.name.ToLower().Contains("floor"));
                
                if (floorTilemap != null)
                {
                    // Add floor collider
                    AddFloorCollider(floorTilemap.gameObject);
                }
                else
                {
                    Debug.LogWarning($"Could not find floor tilemap in room {roomInstance.Room.GetDisplayName()}");
                }

                floorTilemap.AddComponent<CurrentRoomDetectionTriggerHandler>();

                // Add the room manager component
                var roomManager = roomTemplateInstance.AddComponent<RoomManager>();
                var combatRoomComponent = roomTemplateInstance.GetComponent<CombatRoom>();
                roomManager.RoomInstance = roomInstance;
                Debug.Log("Room instance: " + roomInstance.Room.GetDisplayName());
                if (combatRoomComponent != null)
                {
                    roomManager.CombatRoomComponent = combatRoomComponent;
                }
            }
        }

        private void AddFloorCollider(GameObject floor)
        {
            var tilemapCollider2D = floor.AddComponent<TilemapCollider2D>();
            tilemapCollider2D.compositeOperation = Collider2D.CompositeOperation.Merge;

            var compositeCollider2d = floor.AddComponent<CompositeCollider2D>();
            compositeCollider2d.geometryType = CompositeCollider2D.GeometryType.Polygons;
            compositeCollider2d.isTrigger = true;
            compositeCollider2d.generationType = CompositeCollider2D.GenerationType.Manual;

            floor.GetComponent<Rigidbody2D>().bodyType = RigidbodyType2D.Static;
        }
    }
}
