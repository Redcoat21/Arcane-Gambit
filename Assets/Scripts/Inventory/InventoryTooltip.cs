using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;
using Player;

public class InventoryTooltip : MonoBehaviour
{
    public static InventoryTooltip Instance; // Singleton reference

    [SerializeField] private RectTransform tooltipPanel;
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI descriptionText;
    [SerializeField] private Transform statsContainer;
    [SerializeField] private GameObject statLinePrefab;

    private void Awake()
    {
        Instance = this;
        Hide();
    }

    private void Update()
    {
        if (tooltipPanel.gameObject.activeSelf)
        {
            Vector2 mousePos = Input.mousePosition;
            tooltipPanel.position = mousePos + new Vector2(-200, 0); // Left side of cursor
        }
    }

    public void Show(ItemData itemData, int quantity)
    {
        nameText.text = itemData.itemName;
        descriptionText.text = itemData.description;

        foreach (Transform child in statsContainer)
            Destroy(child.gameObject);

        void AddStat(string label, float value, bool isPercentage = false)
        {
            if (value == 0) return;

            float total = value * quantity;

            string valueStr = (value > 0 ? "+" : "") + value.ToString();
            string totalStr = (total > 0 ? "+" : "") + total.ToString();

            if (isPercentage)
            {
                valueStr += "%";
                totalStr += "%";
            }

            string formatted = $"{label}: {valueStr} ({totalStr})";

            var statGO = Instantiate(statLinePrefab.gameObject, statsContainer);
            var statText = statGO.GetComponent<TextMeshProUGUI>();
            statText.text = formatted;
            statText.color = value >= 0 ? Color.green : Color.red;
        }

        AddStat("HP Bonus", itemData.hpModifier);
        AddStat("Mana Bonus", itemData.manaModifier);
        AddStat("Move Speed", itemData.moveSpeedModifier/100);
        AddStat("Attack", itemData.attackModifier);
        AddStat("Melee Damage", itemData.meleeDamageModifier, true);
        AddStat("Ranged Damage", itemData.rangedDamageModifier, true);
        AddStat("Elemental Damage", itemData.elementalDamageModifier, true);
        tooltipPanel.gameObject.SetActive(true);
        LayoutRebuilder.ForceRebuildLayoutImmediate(tooltipPanel);
    }

    public void Hide()
    {
        tooltipPanel.gameObject.SetActive(false);
    }
}
