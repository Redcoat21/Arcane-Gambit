using System.Collections;
using Player;
using UnityEngine;

namespace Enemy
{
    public class EnemyGoldDropComponent : MonoBehaviour
    {
        [SerializeField] 
        private int minGoldDrop = 1;
        
        [SerializeField] 
        private int maxGoldDrop = 5;
        
        [SerializeField]
        private GameObject goldPrefab;
        
        [SerializeField]
        private float goldCollectionRadius = 1.5f;
        
        [SerializeField]
        private float attractionSpeed = 5f;
        
        [SerializeField]
        private float attractionDelay = 0.5f;

        private void Awake()
        {
            // Get the EnemyScript component
            EnemyScript enemyScript = GetComponent<EnemyScript>();
            
            if (enemyScript == null)
            {
                Debug.LogError("No EnemyScript found on the same GameObject! Gold drops won't work.", this);
            }
        }
        
        // This method will be called by EnemyScript when the enemy dies
        public void DropGold()
        {
            int goldAmount = Random.Range(minGoldDrop, maxGoldDrop + 1);

            if (goldPrefab != null)
            {
                // Instantiate gold object at the enemy's position with slight randomization
                Vector3 dropPosition = transform.position + new Vector3(
                    Random.Range(-0.5f, 0.5f),
                    Random.Range(-0.5f, 0.5f),
                    0f
                );

                GameObject goldInstance = Instantiate(goldPrefab, dropPosition, Quaternion.identity);
                GoldPickupComponent pickupComponent = goldInstance.GetComponent<GoldPickupComponent>();
                
                if (pickupComponent != null)
                {
                    pickupComponent.SetGoldAmount(goldAmount);
                    StartCoroutine(AttractGoldToPlayerAfterDelay(goldInstance));
                }
                else
                {
                    Debug.LogWarning("Gold prefab doesn't have a GoldPickupComponent!", this);
                }
            }
            else
            {
                Debug.LogWarning("No gold prefab assigned to drop gold!", this);
                
                // Fallback: try to find the player and directly add gold
                PlayerCharacter player = FindFirstObjectByType<PlayerCharacter>();
                if (player != null)
                {
                    CurrencyComponent currency = player.GetComponent<CurrencyComponent>();
                    if (currency != null)
                    {
                        currency.AddGold(goldAmount);
                    }
                }
            }
        }
        
        private IEnumerator AttractGoldToPlayerAfterDelay(GameObject goldObject)
        {
            if (goldObject == null) yield break;

            // Wait for the specified delay before attraction begins
            yield return new WaitForSeconds(attractionDelay);

            while (goldObject != null)
            {
                // Find the player
                PlayerCharacter player = FindFirstObjectByType<PlayerCharacter>();
                if (player == null) yield break;

                // Calculate distance to player
                float distanceToPlayer = Vector3.Distance(player.transform.position, goldObject.transform.position);

                // If player is within collection radius, move gold toward player
                if (distanceToPlayer <= goldCollectionRadius)
                {
                    goldObject.transform.position = Vector3.MoveTowards(
                        goldObject.transform.position,
                        player.transform.position,
                        attractionSpeed * Time.deltaTime
                    );
                }

                yield return null;
            }
        }
    }
}
