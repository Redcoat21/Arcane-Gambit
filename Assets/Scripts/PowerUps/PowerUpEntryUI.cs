using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PowerupEntryUI : MonoBehaviour
{
    public Image backgroundImage;
    public Image selectedIndicator;
    public TextMeshProUGUI descriptionText;

    private PowerupData powerupData;
    private bool isSelected = false;
    private PowerupListManager listManager;

    public Sprite unselectedSprite;
    public Sprite selectedSprite;

    public void Setup(PowerupData data, PowerupListManager manager)
    {
        powerupData = data;
        listManager = manager;
        descriptionText.text = data.description;
        SetSelected(false);
    }

    public void OnClick()
    {
        listManager.TogglePowerupSelection(this);
    }

    public void SetSelected(bool selected)
    {
        isSelected = selected;
        backgroundImage.sprite = selected ? selectedSprite : unselectedSprite;
        selectedIndicator.enabled = selected;
    }

    public bool IsSelected() => isSelected;
    public PowerupData GetData() => powerupData;
}
