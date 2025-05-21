using UnityEngine;

namespace Components.MeleeDamage
{
    public class MeleeDamageComponent : MonoBehaviour
    {
        [SerializeField] [Range(0f, 100000f)] private float meleeMultiplier = 0.5f;

        public float MeleeMultiplier
        {
            get => meleeMultiplier;
            set => meleeMultiplier = Mathf.Max(0f, value);
        }
    }
}