using UnityEngine;

namespace Enemy
{
    [ExecuteAlways]
    public class SpawnMarker : MonoBehaviour
    {
        [SerializeField]
        private SpriteRenderer spriteRenderer;
    
        private void OnEnable()
        {
            enabled = false;
        }
    }
}
