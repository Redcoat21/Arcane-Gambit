using Edgar.Unity;
using NavMeshPlus.Components;
using UnityEngine;

namespace Enemy
{
    public class NavMeshBakerComponent : DungeonGeneratorPostProcessingComponentGrid2D
    {
        public override void Run(DungeonGeneratorLevelGrid2D level)
        {
            var rooms = level.RoomInstances;
            foreach (var room in rooms)
            {
                var navigationGameObject = room.RoomTemplateInstance.transform.Find("Navigation")?.gameObject;
                if (navigationGameObject != null)
                {
                    var navigationSurface = navigationGameObject?.GetComponent<NavMeshSurface>();
                    if (Application.isPlaying)
                    {
                        navigationSurface.BuildNavMeshAsync();
                    }
                    else
                    {
                        navigationSurface.BuildNavMesh();
                    }
                }
                else
                {
                    Debug.LogWarning("No navigation surface found in " + room.RoomTemplateInstance.name);
                }
            }
        }
    }
}
