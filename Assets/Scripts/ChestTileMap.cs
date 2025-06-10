using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Player;

public class TilemapChest : MonoBehaviour
{
    [SerializeField] private List<ItemData> possibleItems; // Set in Inspector
    [SerializeField] private float interactionRange = 2f;
    [SerializeField] private Tilemap chestTilemap;         // Assign in Inspector
    [SerializeField] private Vector3Int chestTilePos;      // Tile position to remove

    private bool playerNearby;
    private PlayerCharacter player;

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log(player);
            OpenChest();
        }
    }

    private void OpenChest()
    {
        if (possibleItems.Count == 0 || player == null) return;

        int index = Random.Range(0, possibleItems.Count);
        ItemData item = possibleItems[index];

        var inventory = player.GetComponent<InventoryComponent>();
        if (inventory != null)
        {
            inventory.AddItem(item);
        }

        // Remove the tile from the tilemap
        if (chestTilemap != null)
        {
            chestTilemap.SetTile(chestTilePos, null);
        }

        // Destroy this trigger object (not the tile itself)
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
