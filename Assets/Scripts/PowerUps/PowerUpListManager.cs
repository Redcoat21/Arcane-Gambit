using UnityEngine;
using System.Collections.Generic;

public class PowerupListManager : MonoBehaviour
{
    public Transform contentParent;
    public GameObject powerupPrefab;
    public PowerupData[] powerups; // Assign 10 PowerupData assets

    private List<PowerupEntryUI> selectedPowerups = new List<PowerupEntryUI>();
    private const int maxSelection = 6;

    void Start()
    {
        foreach (var powerup in powerups)
        {
            GameObject obj = Instantiate(powerupPrefab, contentParent);
            PowerupEntryUI entry = obj.GetComponent<PowerupEntryUI>();
            entry.Setup(powerup, this);
        }
    }

    public void TogglePowerupSelection(PowerupEntryUI entry)
    {
        if (entry.IsSelected())
        {
            selectedPowerups.Remove(entry);
            entry.SetSelected(false);
        }
        else if (selectedPowerups.Count < maxSelection)
        {
            selectedPowerups.Add(entry);
            entry.SetSelected(true);
        }
        else
        {
            Debug.Log("You can only select up to 6 powerups.");
        }
    }
}
