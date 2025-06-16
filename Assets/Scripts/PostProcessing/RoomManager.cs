using System.Collections.Generic;
using System.Linq;
using Edgar.Unity;
using Levels;
using Levels.Room;
using UnityEngine;

namespace PostProcessing
{
    public class RoomManager : MonoBehaviour
    {
        public RoomInstanceGrid2D RoomInstance { get; set; }
        public CombatRoom CombatRoomComponent { get; set; }

        private bool hasBeenEntered = false;

        public void OnRoomEnter(GameObject otherColliderGameObject)
        {
            if (!hasBeenEntered)
            {
                var enemySpawns = RoomInstance.RoomTemplateInstance.gameObject.transform.Find("EnemySpawns");

                if (enemySpawns != null)
                {
                    var spawnMarkers = GetRandomSpawnMarkers(GetSpawnMarkers(enemySpawns), CombatRoomComponent.EnemyCount);
                    SpawnEnemies(spawnMarkers);
                }
                else
                {
                    Debug.LogWarning("EnemySpawns not found!");
                }

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
            hasBeenEntered = true;
        }

        public void OnRoomLeave(GameObject otherColliderGameObject)
        {
            Debug.Log($"Room leave {RoomInstance.Room.GetDisplayName()}");
            // Handle any cleanup when leaving the room
        }

        private List<Transform> GetSpawnMarkers(Transform parent)
        {
            var children = new List<Transform>();
            foreach (Transform child in parent)
            {
                children.Add(child);
            }

            return children;
        }

        private List<Transform> GetRandomSpawnMarkers(List<Transform> spawnMarkers, int count)
        {
            if (count <= 0 || spawnMarkers.Count == 0)
            {
                return new List<Transform>();
            }

            // If the requested count is greater than the available markers, return all markers
            if (count >= spawnMarkers.Count)
            {
                return spawnMarkers;
            }

            var randomMarkers = spawnMarkers.OrderBy(x => Random.value).Take(count).ToList();
            return randomMarkers;
        }

        private void SpawnEnemies(List<Transform> spawnMarkers)
        {
            int totalCount = spawnMarkers.Count;
            int meleeEnemyCount = Random.Range(1, totalCount);
            int rangedEnemyCount = totalCount - meleeEnemyCount;

            List<GameObject> enemies = new List<GameObject>();

            for (int i = 0; i < meleeEnemyCount; i++)
            {
                var meleeEnemy = EnemyPoolManager.Instance.Get("melee");
                enemies.Add(meleeEnemy);
            }

            foreach (var enemy in enemies)
            {
                enemy.transform.position = spawnMarkers[Random.Range(0, spawnMarkers.Count)].position;
                enemy.SetActive(true);
            }

        }
    }
}
