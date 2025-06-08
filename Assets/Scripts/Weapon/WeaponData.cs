using UnityEngine;

[CreateAssetMenu(fileName = "New Weapon", menuName = "Inventory/Weapon")]
public class WeaponData : ScriptableObject
{
    public string weaponName;
    [TextArea] public string description; // 🆕 Description field
    public Sprite weaponSprite;
    public int attack;
    public bool isMelee;
    public Rarity rarity;
    public float attackSpeed;

    public float DPS => attack / attackSpeed;

    public string GetStatDescription()
    {
        string type = isMelee ? "Melee" : "Ranged";
        return $"ATK: {attack}\n" +
               $"Type: {type}\n" +
               $"Rarity: {rarity}\n" +
               $"Attack Speed: {attackSpeed}s\n" +
               $"DPS: {DPS:F1}";
    }
}