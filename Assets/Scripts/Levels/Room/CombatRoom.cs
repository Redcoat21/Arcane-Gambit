using UnityEngine;

namespace Levels.Room
{
    public class CombatRoom : MonoBehaviour
    {
        [SerializeField]
        [Range(1, 5)]
        private int waveNumber = 2;

        [SerializeField]
        [Range(1, 30)]
        private int enemyCount = 10;

        public int WaveNumber
        {
            get => waveNumber;
            set
            {
                if (value < 1 || value > 5)
                {
                    Debug.LogWarning("Wave number must be between 1 and 5.");
                    return;
                }

                waveNumber = value;
            }
        }
        
        public int EnemyCount
        {
            get => enemyCount;
            set
            {
                if (value < 1 || value > 30)
                {
                    Debug.LogWarning("Enemy count must be between 1 and 30.");
                    return;
                }

                enemyCount = value;
            }
        }
    }
}
