using UnityEngine;

namespace Components.RangedDamage
{
    public class RangedDamageComponent : MonoBehaviour
    {
        [SerializeField] [Range(0f, 100000f)] private float rangedMultiplier = 0.5f;

        public float RangedMultiplier
        {
            get => rangedMultiplier;
            set => rangedMultiplier = Mathf.Max(0f, value);
        }
    }
}