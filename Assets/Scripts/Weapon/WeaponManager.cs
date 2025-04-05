using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class WeaponManager : MonoBehaviour
{
    [Header("Weapon List")]
    public Weapon[] weapons;

    [Header("UI References")]
    public Image weaponImage;
    public TextMeshProUGUI weaponNameText;
    public TextMeshProUGUI weaponStatsText;
    public Button leftButton;
    public Button rightButton;

    private int currentIndex = 0;

    void Start()
    {
        UpdateUI();
        leftButton.onClick.AddListener(PrevWeapon);
        rightButton.onClick.AddListener(NextWeapon);
    }

    void PrevWeapon()
    {
        currentIndex = (currentIndex - 1 + weapons.Length) % weapons.Length;
        UpdateUI();
    }

    void NextWeapon()
    {
        currentIndex = (currentIndex + 1) % weapons.Length;
        UpdateUI();
    }

    void UpdateUI()
    {
        Weapon currentWeapon = weapons[currentIndex];
        weaponImage.sprite = currentWeapon.weaponSprite;
        weaponNameText.text = currentWeapon.weaponName;
        weaponStatsText.text = currentWeapon.GetStatDescription();
    }
}
