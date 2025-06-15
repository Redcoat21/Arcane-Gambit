using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Attack;
using UnityEngine.Serialization;

public class AttackUI : MonoBehaviour
{
    [FormerlySerializedAs("attackComponent")]
    [SerializeField] private AttackComponentTemp attackComponentTemp;
    [SerializeField] private TextMeshProUGUI attackText;
    [SerializeField] private Image attackIndicatorImage;

    private void Start()
    {
        if (attackComponentTemp != null)
        {
            UpdateUI();
            attackComponentTemp.OnAttackPerformed += HandleAttackPerformed;
            attackComponentTemp.OnAttackChanged += HandleAttackChanged;
        }
    }

    private void OnDestroy()
    {
        if (attackComponentTemp != null)
        {
            attackComponentTemp.OnAttackPerformed -= HandleAttackPerformed;
            attackComponentTemp.OnAttackChanged -= HandleAttackChanged;
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
        if (attackText != null && attackComponentTemp != null)
        {
            attackText.text = $"Attack: {attackComponentTemp.BaseAttackDamage}";
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
