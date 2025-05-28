using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Player;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private InventoryComponent inventoryComponent;
    [SerializeField] private GameObject itemSlotPrefab;
    [SerializeField] private Transform contentPanel; // Parent container (like a GridLayout)

    private readonly List<GameObject> itemSlots = new();

    private void OnEnable()
    {
        inventoryComponent.OnInventoryChanged += RefreshUI;
        RefreshUI();
    }

    private void OnDisable()
    {
        inventoryComponent.OnInventoryChanged -= RefreshUI;
    }

    private void RefreshUI()
    {
        // Clear existing slots
        foreach (var slot in itemSlots)
            Destroy(slot);
        itemSlots.Clear();

        // Rebuild UI
        foreach (InventoryItem item in inventoryComponent.GetItems())
        {
            GameObject slot = Instantiate(itemSlotPrefab, contentPanel);
            Image icon = slot.transform.Find("Icon").GetComponent<Image>();
            TextMeshProUGUI quantityText = slot.transform.Find("QuantityText").GetComponent<TextMeshProUGUI>();

            slot.GetComponent<InventoryItemSlotUI>().Setup(item);
            
            icon.sprite = item.itemData.icon;
            quantityText.text = "x" + item.quantity;

            itemSlots.Add(slot);
        }
    }
}
