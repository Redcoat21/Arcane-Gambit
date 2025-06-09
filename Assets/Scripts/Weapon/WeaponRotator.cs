using UnityEngine;

public class WeaponRotator : MonoBehaviour
{
    public Transform pivot;
    public float rotationSpeed = 360f;
    public float duration = 1f;

    private float timer;

    void Update()
    {
        if (pivot == null) return;

        timer += Time.deltaTime;
        if (timer >= duration)
        {
            Destroy(gameObject); // Clean up
            return;
        }

        transform.RotateAround(pivot.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}
