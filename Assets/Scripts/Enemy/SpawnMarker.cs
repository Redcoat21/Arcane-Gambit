using UnityEngine;

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
