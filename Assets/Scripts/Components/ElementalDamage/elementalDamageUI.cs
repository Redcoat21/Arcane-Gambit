using UnityEngine;
using TMPro;
using Components.ElementalDamage;

public class ElementalDamageUI : MonoBehaviour
{
    [SerializeField] private ElementalDamageComponent elementalDamageComponent;
    [SerializeField] private TextMeshProUGUI elementalText;

    private void Start()
    {
        if (elementalDamageComponent != null && elementalText != null)
            UpdateUI();
    }

    public void UpdateUI()
    {
        elementalText.text = $"Elemental Damage: {elementalDamageComponent.ElementalMultiplier:F1}%";
    }
}
