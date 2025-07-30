using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string weaponName;
    public Sprite weaponSprite;
    public int attack;
    public bool isMelee; // true = melee, false = ranged
    public Rarity rarity;

    [Tooltip("How many seconds does 1 attack take")]
    public float attackSpeed; // e.g., 1.5 means 1.5 attacks per second

    public float DPS => attack / attackSpeed;

    public string GetStatDescription()
    {
        string type = isMelee ? "Melee" : "Ranged";
        return $"ATK: {attack}\n" +
               $"Type: {type}\n" +
               $"Rarity: {rarity}\n" +
               $"Speed: {attackSpeed} atk/s\n" +
               $"DPS: {DPS:F1}";
    }
}