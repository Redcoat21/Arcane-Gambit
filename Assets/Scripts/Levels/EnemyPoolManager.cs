using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace Levels
{
    [Serializable]
    public class EnemyPoolEntry
    {
        [SerializeField]
        private string enemyType;

        [SerializeField]
        private GameObject prefab;

        [SerializeField]
        private int preloadAmount = 5;

        public string EnemyType => enemyType;
        public GameObject Prefab => prefab;
        public int PreloadAmount => preloadAmount;
    }

    /// <summary>
    /// Used to manage the pool of enemies in the game.
    /// </summary>
    public class EnemyPoolManager : MonoBehaviour
    {
        public static EnemyPoolManager Instance { get; private set; }
        
        [SerializeField]
        private List<EnemyPoolEntry> enemyTypes;
        
        private Dictionary<string, ObjectPool<GameObject>> pools = new Dictionary<string, ObjectPool<GameObject>>();

        public void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
            
            foreach (var entry in enemyTypes)
            {
                var prefab = entry.Prefab;
                var type = entry.EnemyType;

                var pool = new ObjectPool<GameObject>(
                    () => Instantiate(prefab),
                    enemy => enemy.SetActive(true),
                    enemy => enemy.SetActive(false),
                    enemy => Destroy(enemy),
                    true, // collectionCheck
                    entry.PreloadAmount, // default capacity
                    100 // max size (you can adjust)
                );

                pools[type] = pool;

                // Pre-warm the pool
                for (int i = 0; i < entry.PreloadAmount; i++)
                {
                    var obj = pool.Get();
                    pool.Release(obj);
                }
            }
        }
        
        public GameObject Get(string enemyType)
        {
            if (!pools.TryGetValue(enemyType, out var pool))
            {
                Debug.LogError($"Enemy type '{enemyType}' not found.");
                return null;
            }

            return pool.Get();
        }

        public void Return(string enemyType, GameObject enemy)
        {
            if (pools.TryGetValue(enemyType, out var pool))
            {
                pool.Release(enemy);
            }
            else
            {
                Destroy(enemy); // fallback
            }
        }

    }
}
