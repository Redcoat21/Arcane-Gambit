using System.Collections.Generic;
using Edgar.Unity;
using Enemy;
using UnityEngine;
using UnityEngine.Pool;
using Random = UnityEngine.Random;

namespace DefaultNamespace
{
    public class PooledEnemy
    {
        public EnemyScript Enemy { get; set; }
        public bool IsActive { get; set; }

        public PooledEnemy(EnemyScript enemy, bool isActive = false)
        {
            Enemy = enemy;
            IsActive = isActive;
        }
    }

    public class RoomManager : MonoBehaviour
    {
        // Singleton implementation
        private static RoomManager _instance;
        public static RoomManager Instance
        {
            get { return _instance; }
        }

        // Room properties
        public RoomInstanceGrid2D RoomInstance { get; private set; }

        [Range(1, 100)]
        public int enemySpawnChance = 50;

        [Range(0, 30)]
        public int maximumNumberOfEnemiesToSpawn = 10;

        [Range(1, 30)]
        public int minimumNumberOfEnemiesToSpawn = 1;

        [SerializeField]
        private List<EnemyScript> enemiesToSpawn;

        private Dictionary<EnemyScript, ObjectPool<GameObject>> enemyPools =
            new Dictionary<EnemyScript, ObjectPool<GameObject>>();

        private List<GameObject> activeEnemies = new List<GameObject>();

        private List<PooledEnemy> enemiesPool = new List<PooledEnemy>();

        private void Awake()
        {
            // Singleton pattern implementation
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            _instance = this;

            // Initialize enemy pools
            InitializeEnemyPools();
        }

        private void InitializeEnemyPools()
        {
            if (enemiesToSpawn == null || enemiesToSpawn.Count == 0)
            {
                Debug.LogWarning("No enemy prefabs assigned to spawn");
                return;
            }

            foreach (var enemyPrefab in enemiesToSpawn)
            {
                if (enemyPrefab == null) continue;

                enemyPools[enemyPrefab] = new ObjectPool<GameObject>(
                    createFunc: () =>
                    {
                        var enemy = Instantiate(enemyPrefab.gameObject);
                        enemy.SetActive(false);
                        return enemy;
                    },
                    actionOnGet: (obj) => obj.SetActive(true),
                    actionOnRelease: (obj) => obj.SetActive(false),
                    actionOnDestroy: (obj) => Destroy(obj),
                    defaultCapacity: 10,
                    maxSize: 30
                );
            }
        }

        public void SetRoomInstance(RoomInstanceGrid2D roomInstance)
        {
            RoomInstance = roomInstance;
        }

        private void SpawnEnemies(Transform roomTransform)
        {
            if (enemiesToSpawn.Count == 0 || enemyPools.Count == 0)
            {
                Debug.LogError("No enemy prefabs available to spawn");
                return;
            }

            int markerActivated = 0;
            var enemiesSpawner = roomTransform.Find("EnemySpawns");

            if (enemiesSpawner == null)
            {
                Debug.Log($"No enemy spawner found in {roomTransform.name}");
                // No enemy spawner found in this room, maybe it's a safe room or a shop room.
                return;
            }

            if (enemiesSpawner.childCount < minimumNumberOfEnemiesToSpawn)
            {
                Debug.Log("There's not enough enemy spawners in this room, activating all spawner instead");
                foreach (Transform enemySpawner in enemiesSpawner)
                {
                    var marker = enemySpawner.gameObject;

                    SpawnEnemy(marker.transform.position, marker.transform);
                    markerActivated++;
                }
            }
            else
            {
                foreach (Transform enemySpawner in enemiesSpawner)
                {
                    var marker = enemySpawner.gameObject;

                    if (Random.Range(0, 100) < enemySpawnChance && markerActivated < maximumNumberOfEnemiesToSpawn)
                    {
                        SpawnEnemy(marker.transform.position, marker.transform);
                        marker.SetActive(true);
                        markerActivated++;
                    }
                    else
                    {
                        marker.SetActive(false);
                    }
                }

                while (markerActivated < minimumNumberOfEnemiesToSpawn)
                {
                    Debug.Log($"Not enough enemies spawned, activating additional spawners");
                    var marker = enemiesSpawner.GetChild(Random.Range(0, enemiesSpawner.childCount)).gameObject;

                    if (marker.activeSelf == false)
                    {
                        SpawnEnemy(marker.transform.position, marker.transform);
                        marker.SetActive(true);
                        markerActivated++;
                    }
                }
            }
        }

        private void SpawnEnemy(Vector3 position, Transform parent)
        {
            if (enemiesToSpawn.Count == 0 || enemyPools.Count == 0)
            {
                Debug.LogError("No enemy prefabs available to spawn");
                return;
            }

            // Choose a random enemy type to spawn
            var enemyPrefab = enemiesToSpawn[Random.Range(0, enemiesToSpawn.Count)];

            // Get an enemy from the pool
            var enemyInstance = enemyPools[enemyPrefab].Get();

            // Position and parent the enemy
            enemyInstance.transform.position = position;
            enemyInstance.transform.SetParent(parent);

            // Keep track of active enemies
            activeEnemies.Add(enemyInstance);

            if (parent.GetComponent<SpriteRenderer>())
            {
                parent.GetComponent<SpriteRenderer>().enabled = false;
            }
        }

        public void ReturnEnemyToPool(GameObject enemy)
        {
            foreach (var pool in enemyPools.Values)
            {
                // Try to release to each pool until one accepts it
                try
                {
                    pool.Release(enemy);
                    activeEnemies.Remove(enemy);
                    return;
                }
                catch
                {
                    // Not from this pool, try the next one
                }
            }
        }

        public void OnRoomEnter(GameObject otherColliderGameObject)
        {
            // Debug.Log($"Room enter {RoomInstance.Room.GetDisplayName()}");
            //
            // if (RoomInstance != null)
            // {
            //     // Spawn enemies in the room when player enters
            //     SpawnEnemies(RoomInstance.RoomTemplateInstance.transform);
            //
            //     // Notify room detection manager
            //     if (Instance)
            //     {
            //         Instance.OnRoomEnter(RoomInstance);
            //     }
            // }
            // else
            // {
            //     Debug.LogError("Room instance is null in OnRoomEnter");
            // }
        }

        public void OnRoomLeave(GameObject otherColliderGameObject)
        {
        }
    }
}
