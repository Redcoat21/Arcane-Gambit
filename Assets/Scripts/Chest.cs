using System.Collections.Generic;
using UnityEngine;
using Player;

public class Chest : MonoBehaviour
{
    [SerializeField] private List<ItemData> possibleItems; // Assign in Inspector
    [SerializeField] private float interactionRange = 2f;

    private bool playerNearby;
    private PlayerCharacter player;

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (possibleItems.Count == 0 || player == null) return;

        // Choose a random item
        int index = Random.Range(0, possibleItems.Count);
        ItemData item = possibleItems[index];

        // Add item to player's inventory
        var inventory = player.GetComponent<InventoryComponent>();
        if (inventory != null)
        {
            inventory.AddItem(item);
        }

        // Optional: Add animation, sound, or VFX here

        // Destroy the chest
        Destroy(gameObject);
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
        }
    }
}
