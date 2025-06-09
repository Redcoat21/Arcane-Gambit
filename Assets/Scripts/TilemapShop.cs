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
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;

    [Header("Shop Settings")]
    [SerializeField] private List<ConsumableData> possiblePotions; // List of all potions
    [SerializeField] private int potionCost = 100;

    private bool playerNearby = false;
    private PlayerCharacter player;
    private ConsumableData currentPotion;

    private void Start()
    {
        shopDialogPanel.SetActive(false);

        yesButton.onClick.AddListener(() =>
        {
            TryBuyPotion();
            shopDialogPanel.SetActive(false);
        });

        noButton.onClick.AddListener(() =>
        {
            shopDialogPanel.SetActive(false);
        });
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            ShowDialog();
        }
    }

    private void ShowDialog()
    {
        if (possiblePotions.Count == 0) return;

        // Pick a random potion
        currentPotion = possiblePotions[Random.Range(0, possiblePotions.Count)];

        // Show dialog text
        // shopDialogText.text = $"Buy <b>{currentPotion.itemName}</b> for {potionCost} coins?\n\n{currentPotion.GetStatDescription()}";
        shopDialogPanel.SetActive(true);
    }

    private void TryBuyPotion()
    {
        var coinComponent = player.GetComponent<CurrencyComponent>();

        if (coinComponent != null && coinComponent.CurrentGold >= potionCost)
        {
            coinComponent.CurrentGold -= potionCost;
            player.consumable = currentPotion; // just assign it here
            shopDialogText.text = $"Very well then, enjoy your {currentPotion.itemName}!";
            Debug.Log($"Bought {currentPotion.itemName}");
        }
        else
        {
            shopDialogText.text = $"Seems to me, you're short on cash!";
            Debug.Log("Not enough coins!");
        }
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
            playerNearby = false;
            player = null;
            shopDialogPanel.SetActive(false);
        }
    }
}
