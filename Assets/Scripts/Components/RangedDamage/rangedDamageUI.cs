using UnityEngine;
using TMPro;
using Components.RangedDamage;

public class RangedDamageUI : MonoBehaviour
{
    [SerializeField] private RangedDamageComponent rangedDamageComponent;
    [SerializeField] private TextMeshProUGUI rangedText;

    private void Start()
    {
        if (rangedDamageComponent != null && rangedText != null)
            UpdateUI();
    }

    public void UpdateUI()
    {
        rangedText.text = $"Ranged Damage: {rangedDamageComponent.RangedMultiplier:F1}%";
    }
}
