using UnityEngine;
using UnityEngine.UI;

public class SpellUI : MonoBehaviour
{
    public static SpellUI Instance { get; private set; }

    [SerializeField] private Image spellIcon;

    private float lastCastTime = -999f;
    private float spellCooldown = 0f;
    private bool isOnCooldown = false;

    private void Awake()
    {
        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        // }
        // else
        // {
            Instance = this;
        // }
    }

    public void UpdateSpellUI(SpellData spell)
    {
        if (spellIcon != null && spell.spellSprite != null)
        {
            spellIcon.sprite = spell.spellSprite;
            spellIcon.color = new Color(1f, 1f, 1f, 1f); // fully visible
        }

        spellCooldown = spell.cooldown;
        lastCastTime = -spell.cooldown; // allow immediate cast
        isOnCooldown = false;
    }

    public void TriggerCooldown()
    {
        lastCastTime = Time.time;
        isOnCooldown = true;
    }

    private void Update()
    {
        if (!isOnCooldown || spellCooldown <= 0f || spellIcon == null)
            return;

        float timeSinceCast = Time.time - lastCastTime;
        float t = Mathf.Clamp01(timeSinceCast / spellCooldown); // 0 = just cast, 1 = ready

        // Set opacity from 0.4 (not ready) to 1.0 (ready)
        float alpha = Mathf.Lerp(0.4f, 1f, t);
        Color iconColor = spellIcon.color;
        iconColor.a = alpha;
        spellIcon.color = iconColor;

        if (t >= 1f)
        {
            isOnCooldown = false;
        }
    }
}
