using UnityEngine;

[System.Serializable]
public class Weapon
{
    public string weaponName;
    public Sprite weaponSprite;
    public int attack;
    [Range(0, 100)] public int damagePercent; // How much is meelee/ranged
    public bool isMelee; // true = meelee, false = ranged

    public string GetStatDescription()
    {
        string type = isMelee ? "Melee" : "Ranged";
        return $"ATK: {attack}\nType: {type} ({damagePercent}%)";
    }
}