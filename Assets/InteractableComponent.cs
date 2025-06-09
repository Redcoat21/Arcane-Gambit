using Player;
using UnityEngine;
using System;

public class InteractableComponent : MonoBehaviour
{
    private bool playerNearby;
    private PlayerCharacter player;
    
    // Action that will be invoked when the player interacts with this object
    public Action OnInteract;
    
    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            // Invoke the OnInteract action if it has subscribers
            OnInteract?.Invoke();
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
            player = null;
            playerNearby = false;
        }
    }
}
