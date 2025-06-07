using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Mana;

public class ManaUI : MonoBehaviour
{
    [SerializeField] private ManaComponent manaComponent;
    [SerializeField] private TextMeshProUGUI manaText;
    [SerializeField] private TextMeshProUGUI manaInventory;
    [SerializeField] private Image manaFillImage;

    private void Start()
    {
        if (manaComponent != null)
        {
            UpdateUI();
            manaComponent.OnManaChanged += HandleManaChanged;
        }
    }

    private void OnDestroy()
    {
        if (manaComponent != null)
        {
            manaComponent.OnManaChanged -= HandleManaChanged;
        }
    }

    private void HandleManaChanged(int currentMana)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        int currentMana = manaComponent.CurrentMana;
        manaInventory.text = $"Max Mana: {manaComponent.MaximumMana}";
        manaText.text = $"{currentMana} / {manaComponent.MaximumMana}";

        if (manaFillImage != null)
        {
            float fillAmount = (float)currentMana / manaComponent.MaximumMana;
            manaFillImage.rectTransform.localScale = new Vector3(fillAmount, 1f, 1f);
        }

        Debug.Log("Base Max Mana FROM UI: " + manaComponent.MaximumMana);
        Debug.Log("Base CURRENT Mana FROM UI: " + manaComponent.CurrentMana);
    }
}