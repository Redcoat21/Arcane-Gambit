using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Components.Movements;

public class DashCooldownUI : MonoBehaviour
{
    [SerializeField] private Image dashImage; // Assign in inspector
    [SerializeField] private GroundMovementComponent movementComponent;

    private void Start()
    {
        dashImage.type = Image.Type.Filled;
        dashImage.fillMethod = Image.FillMethod.Vertical;
        dashImage.fillOrigin = (int)Image.OriginVertical.Bottom;
        dashImage.fillAmount = 1f;
    }

    public void StartCooldown(float cooldownDuration)
    {
        StartCoroutine(UpdateCooldownUI(cooldownDuration));
    }

    private IEnumerator UpdateCooldownUI(float duration)
    {
        float elapsed = 0f;
        dashImage.color = new Color32(0x9F, 0x9F, 0x9F, 0xFF); // Gray
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            dashImage.fillAmount = elapsed / duration;
            yield return null;
        }
        dashImage.fillAmount = 1f;
        dashImage.color = Color.white; // Ready (white)
    }
}
