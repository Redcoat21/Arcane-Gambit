using System.Collections.Generic;
using Edgar.Unity;
using UnityEngine;
using UnityEngine.Pool;
using Enemy;

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

public class EnemySpawnerComponent : DungeonGeneratorPostProcessingComponentGrid2D
{
    [Range(1, 100)]
    public int enemySpawnChance = 50;

    [Range(0, 30)]
    public int maximumNumberOfEnemiesToSpawn = 10;

    [Range(1, 30)]
    public int minimumNumberOfEnemiesToSpawn = 1;

    [SerializeField]
    private List<EnemyScript> enemiesToSpawn;

    private Dictionary<EnemyScript, ObjectPool<GameObject>> enemyPools = new Dictionary<EnemyScript, ObjectPool<GameObject>>();
    private List<GameObject> activeEnemies = new List<GameObject>();


    private List<PooledEnemy> enemiesPool = new List<PooledEnemy>();

    public override void Run(DungeonGeneratorLevelGrid2D level)
    {
        if (enemiesPool.Count == 0)
        {
            foreach (var enemyToSpawn in enemiesToSpawn)
            {
                enemyPools[enemyToSpawn] = new ObjectPool<GameObject>(
                    createFunc: () => Instantiate(enemyToSpawn.gameObject),
                    actionOnGet: obj => obj.SetActive(true),
                    actionOnRelease: obj => obj.SetActive(false),
                    actionOnDestroy: Destroy,
                    defaultCapacity: maximumNumberOfEnemiesToSpawn,
                    maxSize: maximumNumberOfEnemiesToSpawn * 2
                );
            }
        }

        if (minimumNumberOfEnemiesToSpawn > maximumNumberOfEnemiesToSpawn)
        {
            Debug.LogError("Minimum number of enemies to spawn is greater than maximum number of enemies to spawn");
        }
        else
        {
            for (int i = activeEnemies.Count - 1; i >= 0; i--)
            {
                if (activeEnemies[i] != null)
                {
                    activeEnemies[i].SetActive(false);
                }
            }

            activeEnemies.Clear();
            SpawnEnemies(level);
        }
    }

    private void SpawnEnemies(DungeonGeneratorLevelGrid2D level)
    {
        var rooms = level.RoomInstances;
        foreach (var room in rooms)
        {
            int markerActivated = 0;
            var enemiesSpawner = room.RoomTemplateInstance.transform.Find("EnemySpawns");

            if (enemiesSpawner == null)
            {
                Debug.Log($"No enemy spawner found in {room.RoomTemplateInstance.name}");
                // No enemy spawner found in this room, maybe it's a safe room or a shop room.
                continue;
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

                    if (Random.Next(0, 100) < enemySpawnChance && markerActivated < maximumNumberOfEnemiesToSpawn)
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
                    Debug.LogError($"Not enough enemies spawned, activating additional spawners");
                    var marker = enemiesSpawner.GetChild(Random.Next(0, enemiesSpawner.childCount)).gameObject;

                    if (marker.activeSelf == false)
                    {
                        SpawnEnemy(marker.transform.position, marker.transform);
                        marker.SetActive(true);
                        markerActivated++;
                    }
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
        var enemyPrefab = enemiesToSpawn[Random.Next(0, enemiesToSpawn.Count)];

        // Get an enemy from the pool
        var enemyInstance = enemyPools[enemyPrefab].Get();

        // Position and parent the enemy
        enemyInstance.transform.position = position;
        enemyInstance.transform.SetParent(parent);

        // Keep track of active enemies
        activeEnemies.Add(enemyInstance);
        
        parent.GetComponent<SpriteRenderer>().enabled = false;
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
}
