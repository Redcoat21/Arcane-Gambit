using System.Collections.Generic;
using System.Linq;
using Edgar.Unity;
using Enemy;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Levels
{
    public class RoomManager : MonoBehaviour
    {
        [SerializeField]
        private RoomInstanceGrid2D roomInstance;

        private List<SpawnMarker> spawnMarkers = new List<SpawnMarker>();

        public RoomInstanceGrid2D RoomInstance
        {
            get => roomInstance;
            set => roomInstance = value;
        }

        private void Awake()
        {
            var spawner = GameObject.Find("EnemySpawns");
            foreach (Transform spawn in spawner.transform)
            {
                var marker = spawn.GetComponent<SpawnMarker>();
                if (marker != null)
                {
                    spawnMarkers.Add(marker);
                }
            }
        }

        public void OnRoomEnter(GameObject player)
        {
            if (spawnMarkers.Count == 0)
            {
                Debug.LogWarning("This room doesn't have any spawn markers!");
                return;
            }

            var spawns = spawnMarkers.OrderBy(_ => Random.value).Take(5);
            var enumerable = spawns as SpawnMarker[] ?? spawns.ToArray();
            Debug.Log(
                $"Room {roomInstance.RoomTemplateInstance.name} Have {enumerable.Length} possible spawns count 1 is at ${enumerable.First().transform.position}");
        }

        public void OnRoomLeave(GameObject player)
        {
        }
    }

}
