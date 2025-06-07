using UnityEngine;

namespace Components.ElementalDamage
{
    public class ElementalDamageComponent : MonoBehaviour
    {
        [SerializeField] [Range(0f, 100000f)] private float elementalMultiplier = 0.5f;

        public float ElementalMultiplier
        {
            get => elementalMultiplier;
            set => elementalMultiplier = Mathf.Max(0f, value);
        }
    }
}