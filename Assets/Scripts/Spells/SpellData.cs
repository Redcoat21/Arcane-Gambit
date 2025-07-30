using UnityEngine;

[CreateAssetMenu(fileName = "New Spell", menuName = "Inventory/Spell")]
public class SpellData : ScriptableObject
{
    public string spellName;
    [TextArea] public string description;
    public Sprite spellSprite;
    public int attack;
    public int manaCost;
    public float cooldown;
    public Rarity rarity;
    public string effect;
    [Range(1, 100)] public int level;

    public string GetStatDescription()
    {
        return $"ATK: {attack}\n" +
               $"Mana Cost: {manaCost}\n" +
               $"Cooldown: {cooldown}s\n" +
               $"Rarity: {rarity}" +
               $"Level: {level}";
    }
}
