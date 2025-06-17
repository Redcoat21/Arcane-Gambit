using System.Collections.Generic;
using System.Linq;
using Edgar.Unity;
using Edgar.Unity.Examples.Gungeon;
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
                    var allSpawnMarkers = GetSpawnMarkers(enemySpawns);
                    var spawnMarkers = GetRandomSpawnMarkers(allSpawnMarkers, CombatRoomComponent.EnemyCount);
                    SpawnEnemies(spawnMarkers);
                }
                else
                {
                    Debug.LogWarning("EnemySpawns not found!");
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
            int meleeEnemyCount = totalCount;

            var enemies = new List<GameObject>();

            // Spawn the mêlée enemy first
            for (int i = 0; i < meleeEnemyCount; i++)
            {
                var meleeEnemy = EnemyPoolManager.Instance.Get("melee");
                enemies.Add(meleeEnemy);
            }

            foreach (var enemy in enemies)
            {
                var randomPosition = spawnMarkers[Random.Range(0, spawnMarkers.Count)].position;
                var spawnPosition = new Vector3(randomPosition.x, randomPosition.y, randomPosition.z);
                enemy.transform.position = spawnPosition;
                enemy.transform.rotation = Quaternion.Euler(0, 0, 0);
                enemy.SetActive(true);
            }
        }
    }
}
