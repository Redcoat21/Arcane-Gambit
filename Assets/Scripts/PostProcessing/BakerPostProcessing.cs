using Edgar.Unity;
using NavMeshPlus.Components;
using UnityEngine;

namespace PostProcessing
{
    [CreateAssetMenu(fileName = "BakerPostProcessing", menuName = "Level/Nav Mesh Baker", order = 0)]
    public class BakerPostProcessing : DungeonGeneratorPostProcessingGrid2D
    {
        public override async void Run(DungeonGeneratorLevelGrid2D level)
        {
            var rooms = level.RoomInstances;
            foreach (var room in rooms)
            {
                var navigationGameObject = room.RoomTemplateInstance.transform.Find("NavigationMesh")?.gameObject;
                if (navigationGameObject != null)
                {
                    var navigationSurface = navigationGameObject.GetComponent<NavMeshSurface>();
                    // if (Application.isPlaying)
                    // {
                    //     await navigationSurface.BuildNavMeshAsync();
                    // }
                    // else
                    // {
                    //     navigationSurface.BuildNavMesh();
                    // }
                    await navigationSurface.BuildNavMeshAsync();

                }
                else
                {
                    Debug.LogWarning("No navigation surface found in " + room.RoomTemplateInstance.name);
                }
            }
        }
    }
}
