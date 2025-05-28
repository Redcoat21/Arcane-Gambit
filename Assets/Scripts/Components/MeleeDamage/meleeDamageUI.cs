using UnityEngine;
using TMPro;
using Components.MeleeDamage;

public class MeleeDamageUI : MonoBehaviour
{
    [SerializeField] private MeleeDamageComponent meleeDamageComponent;
    [SerializeField] private TextMeshProUGUI meleeText;

    private void Start()
    {
        if (meleeDamageComponent != null && meleeText != null)
            UpdateUI();
    }

    public void UpdateUI()
    {
        meleeText.text = $"Melee Damage: {meleeDamageComponent.MeleeMultiplier:F1}%";
    }
}