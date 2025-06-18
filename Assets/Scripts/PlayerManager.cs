using System.Collections.Generic;
using UnityEngine;
using Player;

public static class PlayerManager
{
    // Basic Attributes
    public static int MaxHealth { get; set; } = 100;
    public static int CurrentHealth { get; set; } = 100;

    public static int MaxMana { get; set; } = 100;
    public static int CurrentMana { get; set; } = 100;

    public static float MoveSpeed { get; set; } = 1;

    public static int BaseAttack { get; set; } = 100;
    public static float MeleeMultiplier { get; set; } = 0;
    public static float RangedMultiplier { get; set; } = 0;
    public static float ElementalMultiplier { get; set; } = 0;

    public static int Currency { get; set; } = 0;

    // Inventory
    public static List<InventoryItem> InventoryItems { get; private set; } = new();

    // Equipment
    public static WeaponData Weapon1 { get; set; }
    public static WeaponData Weapon2 { get; set; }
    public static ConsumableData Consumable { get; set; }
    public static SpellData EquippedSpell { get; set; }

    public static void SetHealth(int current, int max)
    {
        CurrentHealth = Mathf.Clamp(current, 0, max);
        MaxHealth = max;
    }

    public static void SetMana(int current, int max)
    {
        CurrentMana = Mathf.Clamp(current, 0, max);
        MaxMana = max;
    }

    public static void UpdateInventory(List<InventoryItem> items)
    {
        InventoryItems = new List<InventoryItem>(items);
    }

    public static void AddInventoryItem(ItemData itemData, int quantity)
    {
        InventoryItem existing = InventoryItems.Find(i => i.itemData == itemData);
        if (existing != null)
        {
            existing.quantity += quantity;
        }
        else
        {
            InventoryItems.Add(new InventoryItem(itemData, quantity));
        }
    }

    public static void RemoveInventoryItem(ItemData itemData, int quantity)
    {
        InventoryItem existing = InventoryItems.Find(i => i.itemData == itemData);
        if (existing != null)
        {
            existing.quantity -= quantity;
            if (existing.quantity <= 0)
                InventoryItems.Remove(existing);
        }
    }

    public static void ClearInventory()
    {
        InventoryItems.Clear();
    }
    
    public static void Reset()
    {
        MaxHealth = 100;
        CurrentHealth = 100;

        MaxMana = 100;
        CurrentMana = 100;

        MoveSpeed = 1;

        BaseAttack = 100;
        MeleeMultiplier = 0;
        RangedMultiplier = 0;
        ElementalMultiplier = 0;

        Currency = 0;

        Weapon1 = null;
        Weapon2 = null;
        Consumable = null;
        EquippedSpell = null;

        ClearInventory();
    }

}
