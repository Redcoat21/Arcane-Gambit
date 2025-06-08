using Edgar.Unity;
using Player;
using Unity.Cinemachine;
using UnityEngine;

namespace DefaultNamespace
{
    [CreateAssetMenu(fileName = "CameraPostProcessing", menuName = "Features/Camera Post Processing", order = 0)]
    public class CameraPostProcessing : DungeonGeneratorPostProcessingGrid2D
    {
        public override void Run(DungeonGeneratorLevelGrid2D level)
        {
            var camera = GameObject.Find("Main Camera");
            if (camera == null)
            {
                Debug.LogError("Main Camera not found. Please ensure there is a camera in the scene.");
                return;
            }

            var spawnRoom =
                level.RoomInstances.Find(room => room.RoomTemplateInstance.gameObject.name.Contains("Spawn"));
            if (spawnRoom == null)
            {
                Debug.LogError("Spawn room not found. Please ensure there is a room named 'Spawn' in the level.");
                return;
            }

            var player = spawnRoom.RoomTemplateInstance.gameObject.GetComponentInChildren<PlayerCharacter>();
            if (player == null)
            {
                Debug.LogError(
                    "PlayerCharacter component not found in the spawn room. Please ensure the player is set up correctly.");
                return;
            }

            camera.GetComponent<CinemachineCamera>().Follow = player.transform;
        }
    }
}
