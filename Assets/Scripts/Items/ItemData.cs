using UnityEngine;

public enum Rarity { Common, Uncommon, Rare, Legendary }
public enum Type { Passive, Consumeable }

[CreateAssetMenu(fileName = "New Item", menuName = "Inventory/Item")]
public class ItemData : ScriptableObject
{
public string itemName;
[TextArea] public string description;
public Rarity rarity;
public Sprite icon;
public string status;

public float hpModifier;
public float manaModifier;
public float moveSpeedModifier;
public float attackModifier;
public float meleeDamageModifier;
public float rangedDamageModifier;
public float elementalDamageModifier;

public Type type;
}