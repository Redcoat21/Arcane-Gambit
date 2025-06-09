using Player;
using UnityEngine;

namespace Enemy
{
    public class GoldPickupComponent : MonoBehaviour
    {
        [SerializeField]
        private int goldAmount = 1;
        
        [SerializeField]
        private float destroyDelay = 0.1f;
        
        [SerializeField]
        private AudioClip pickupSound;
        
        private bool hasBeenCollected = false;
        
        public void SetGoldAmount(int amount)
        {
            goldAmount = amount;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (hasBeenCollected) return;
            
            // Check if the player collected the gold
            if (other.CompareTag("Player"))
            {
                CurrencyComponent playerCurrency = other.GetComponent<CurrencyComponent>();
                
                if (playerCurrency != null)
                {
                    // Add gold to player's wallet
                    playerCurrency.AddGold(goldAmount);
                    hasBeenCollected = true;
                    
                    // Play pickup sound if assigned
                    if (pickupSound != null)
                    {
                        AudioSource.PlayClipAtPoint(pickupSound, transform.position);
                    }
                    
                    // Destroy the gold object after a short delay
                    Destroy(gameObject, destroyDelay);
                }
            }
        }
    }
}
