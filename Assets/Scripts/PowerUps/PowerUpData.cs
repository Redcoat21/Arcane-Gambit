using UnityEngine;

[CreateAssetMenu(fileName = "New Powerup", menuName = "Inventory/Powerup")]
public class PowerupData : ScriptableObject
{
    public string powerupName;
    [TextArea] public string description;
}

