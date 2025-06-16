using UnityEngine;

namespace Levels.Room
{
    public class CombatRoom : MonoBehaviour
    {
        [SerializeField]
        [Range(1, 5)]
        private int waveNumber = 2;

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
    }
}
