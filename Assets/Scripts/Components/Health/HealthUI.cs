using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Health;

public class HealthUI : MonoBehaviour
{
    [SerializeField] private HealthComponent healthComponent;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private Image healthFillImage;

    private void Start()
    {
        if (healthComponent != null)
        {
            UpdateUI();
            healthComponent.OnHealthChanged += HandleHealthChanged;
        }
    }

    private void OnDestroy()
    {
        if (healthComponent != null)
        {
            healthComponent.OnHealthChanged -= HandleHealthChanged;
        }
    }

    private void HandleHealthChanged(int currentHP)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        int currentHP = healthComponent.CurrentHealth;
        hpText.text = $"{currentHP} / {healthComponent.MaximumHealth}";

        if (healthFillImage != null)
        {
            float fillAmount = (float)currentHP / healthComponent.MaximumHealth;
            healthFillImage.rectTransform.localScale = new Vector3(fillAmount, 1f, 1f);
        }
        Debug.Log("Base Max Health FROM UI: " + healthComponent.MaximumHealth);
        Debug.Log("Base CURRENT Health FROM UI: " + healthComponent.CurrentHealth);
    }
}
