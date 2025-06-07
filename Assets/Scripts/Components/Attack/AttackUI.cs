using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Attack;

public class AttackUI : MonoBehaviour
{
    [SerializeField] private AttackComponent attackComponent;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Image attackIndicatorImage;

    private void Start()
    {
        if (attackComponent != null)
        {
            UpdateUI();
            attackComponent.OnAttackPerformed += HandleAttackPerformed;
            attackComponent.OnAttackChanged += HandleAttackChanged;
        }
    }

    private void OnDestroy()
    {
        if (attackComponent != null)
        {
            attackComponent.OnAttackPerformed -= HandleAttackPerformed;
            attackComponent.OnAttackChanged -= HandleAttackChanged;
        }
    }

    private void HandleAttackPerformed(int damage)
    {
        // Visual feedback
        FlashAttackIndicator();
        UpdateUI();
    }

    private void HandleAttackChanged(int damage)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (attackText != null && attackComponent != null)
        {
            attackText.text = $"Attack: {attackComponent.BaseAttackDamage}";
        }
    }

    private void FlashAttackIndicator()
    {
        if (attackIndicatorImage != null)
        {
            // Simple flash effect: make it visible for a brief moment
            attackIndicatorImage.enabled = true;
            CancelInvoke(nameof(HideAttackIndicator));
            Invoke(nameof(HideAttackIndicator), 0.2f);
        }
    }

    private void HideAttackIndicator()
    {
        if (attackIndicatorImage != null)
        {
            attackIndicatorImage.enabled = false;
        }
    }
}
