using UnityEngine;

[CreateAssetMenu(fileName = "New Consumable", menuName = "Inventory/Consumable")]
public class ConsumableData : ScriptableObject
{
    public string itemName;
    [TextArea] public string description;
    public Sprite itemSprite;
    public float restoreHealth;
    public float restoreMana;
    public string effect;
    public Rarity rarity;

    public string GetStatDescription()
    {
        return $"Health: +{restoreHealth}%\n" +
               $"Mana: +{restoreMana}%\n" +
               $"Rarity: {rarity}" +
               $"Effect: {effect}";
    }
}
