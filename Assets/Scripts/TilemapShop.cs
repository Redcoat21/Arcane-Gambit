using UnityEngine;
using UnityEngine.UI;
using Player;
using System.Collections.Generic;
using TMPro;

public class TilemapShop : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject shopDialogPanel;
    [SerializeField] private TextMeshProUGUI shopDialogText;
    [SerializeField] private Button buyPotionButton;
    [SerializeField] private Button buyWeaponButton;
    [SerializeField] private Button buySpellButton;
    [SerializeField] private Button leaveButton;

    [Header("Items")]
    [SerializeField] private List<ConsumableData> potions;
    [SerializeField] private List<WeaponData> weapons;
    [SerializeField] private List<SpellData> spells;

    [Header("Prices")]
    [SerializeField] private int potionCost = 100;
    [SerializeField] private int weaponCost = 250;
    [SerializeField] private int spellCost = 200;

    private PlayerCharacter player;
    private bool playerNearby = false;

    private void Start()
    {
        shopDialogPanel.SetActive(false);

        buyPotionButton.onClick.AddListener(BuyPotion);
        buyWeaponButton.onClick.AddListener(BuyWeapon);
        buySpellButton.onClick.AddListener(BuySpell);
        leaveButton.onClick.AddListener(() => shopDialogPanel.SetActive(false));
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            shopDialogPanel.SetActive(true);
            shopDialogText.text = "Welcome to the shop! What would you like to buy?";
        }
    }

    private void BuyPotion()
    {
        if (potions.Count == 0 || player == null) return;

        var potion = potions[Random.Range(0, potions.Count)];
        var coins = player.GetComponent<CurrencyComponent>();
        if (coins == null || coins.CurrentGold < potionCost)
        {
            shopDialogText.text = "Not enough coins for potion.";
            return;
        }

        coins.SpendGold(potionCost);
        player.consumable = potion;
        PlayerManager.Consumable = potion;
        Debug.Log($"Bought potion: {potion.itemName}");

        var healthComponent = player.GetComponent<Components.Health.HealthComponent>();
        var manaComponent = player.GetComponent<Components.Mana.ManaComponent>();

        if (potion.restoreHealth > 0 && healthComponent != null)
        {
            int healthAmount = Mathf.RoundToInt(healthComponent.MaximumHealth * (potion.restoreHealth / 100f));
            healthComponent.CurrentHealth = Mathf.Min(healthComponent.CurrentHealth + healthAmount, healthComponent.MaximumHealth);
        }

        if (potion.restoreMana > 0 && manaComponent != null)
        {
            int manaAmount = Mathf.RoundToInt(manaComponent.MaximumMana * (potion.restoreMana / 100f));
            manaComponent.CurrentMana = Mathf.Min(manaComponent.CurrentMana + manaAmount, manaComponent.MaximumMana);
        }
        player.BuyPotion();

        shopDialogText.text = $"Bought <b>{potion.itemName}</b>!\n{potion.GetStatDescription()}";
    }

    private void BuyWeapon()
    {
        if (weapons.Count == 0 || player == null) return;

        var weapon = weapons[Random.Range(0, weapons.Count)];
        var coins = player.GetComponent<CurrencyComponent>();
        if (coins == null || coins.CurrentGold < weaponCost)
        {
            shopDialogText.text = "Not enough coins for weapon.";
            return;
        }

        coins.SpendGold(weaponCost);
        player.BuyWeapon(weapon);
        PlayerManager.Weapon1 = weapon;
        shopDialogText.text = $"Bought <b>{weapon.weaponName}</b>!\n" +
                      $"ATK: {weapon.attack}, " +
                      $"Type: {(weapon.isMelee ? "Melee" : "Ranged")}, " +
                      $"Rarity: {weapon.rarity}, " +
                      $"Attack Speed: {weapon.attackSpeed}";
    }

    private void BuySpell()
    {
        if (spells.Count == 0 || player == null) return;

        var spell = spells[Random.Range(0, spells.Count)];
        var coins = player.GetComponent<CurrencyComponent>();
        if (coins == null || coins.CurrentGold < spellCost)
        {
            shopDialogText.text = "Not enough coins for spell.";
            return;
        }

        coins.SpendGold(spellCost);
        player.BuySpell(spell);
        PlayerManager.EquippedSpell = spell;
        shopDialogText.text = $"Bought <b>{spell.spellName}</b>!\n{spell.GetStatDescription()}";
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.GetComponent<PlayerCharacter>();
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            player = null;
            playerNearby = false;
            shopDialogPanel.SetActive(false);
        }
    }
}
