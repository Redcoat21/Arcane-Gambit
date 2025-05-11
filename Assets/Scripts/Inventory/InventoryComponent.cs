using System.Collections.Generic;
using UnityEngine;

namespace Player
{
public class InventoryComponent : MonoBehaviour
{
[SerializeField] private List<InventoryItem> items = new();
    public void AddItem(ItemData itemData, int quantity = 1)
    {
        InventoryItem existing = items.Find(i => i.itemData == itemData);

        if (existing != null && itemData.type == Type.Passive)
        {
            existing.quantity += quantity;
        }
        else
        {
            items.Add(new InventoryItem(itemData, quantity));
        }
    }

    public void RemoveItem(ItemData itemData, int quantity = 1)
    {
        InventoryItem existing = items.Find(i => i.itemData == itemData);

        if (existing != null)
        {
            existing.quantity -= quantity;
            if (existing.quantity <= 0)
                items.Remove(existing);
        }
    }

    public List<InventoryItem> GetItems() => items;
    public void AddItemEditor(ItemData item) => AddItem(item, 1);
    public void RemoveItemEditor(ItemData item) => RemoveItem(item, 1);
}

[System.Serializable]
public class InventoryItem
{
    public ItemData itemData;
    public int quantity;

    public InventoryItem(ItemData itemData, int quantity)
    {
        this.itemData = itemData;
        this.quantity = quantity;
    }
}

}