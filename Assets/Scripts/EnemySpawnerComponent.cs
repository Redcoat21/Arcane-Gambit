using System.Collections.Generic;
using Edgar.Unity;
using UnityEngine;

public class EnemySpawnerComponent : DungeonGeneratorPostProcessingComponentGrid2D
{
    [Range(1, 100)]
    public int enemySpawnChance = 50;
    
    [Range(0, 30)]
    public int maximumNumberOfEnemiesToSpawn = 10;
    
    [Range(1, 30)]
    public int minimumNumberOfEnemiesToSpawn = 1;

    [SerializeField]
    private List<Enemy> enemiesSpawnPool;
    
    public override void Run(DungeonGeneratorLevelGrid2D level)
    {
        if (minimumNumberOfEnemiesToSpawn > maximumNumberOfEnemiesToSpawn)
        {
            Debug.LogError("Minimum number of enemies to spawn is greater than maximum number of enemies to spawn");
        }
        else
        {
            SpawnEnemies(level);
        }
    }

    private void SpawnEnemies(DungeonGeneratorLevelGrid2D level)
    {
        var rooms = level.RoomInstances;
        foreach(var room in rooms)
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

                    InstantiateEnemy(marker.transform.position, marker.transform);
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
                        InstantiateEnemy(marker.transform.position, marker.transform);
                        marker.SetActive(true);
                        markerActivated++;
                    }
                    else
                    {
                        marker.SetActive(false);
                    }
                }
                
                while(markerActivated < minimumNumberOfEnemiesToSpawn)
                {
                    Debug.LogError($"Not enough enemies spawned, activating additional spawners");
                    var marker = enemiesSpawner.GetChild(Random.Next(0, enemiesSpawner.childCount)).gameObject;
                    
                    if(marker.activeSelf == false)
                    {
                        InstantiateEnemy(marker.transform.position, marker.transform);
                        marker.SetActive(true);
                        markerActivated++;
                    }
                }
            }
        }
    }

    private void InstantiateEnemy(Vector3 position, Transform parent)
    {
        var enemy = enemiesSpawnPool[Random.Next(0, enemiesSpawnPool.Count)];
        var enemyInstance = Instantiate(enemy, position, Quaternion.identity);
        enemyInstance.transform.SetParent(parent);
        parent.GetComponent<SpriteRenderer>().enabled = false;
    }
}
