using UnityEngine;
using TMPro;

namespace Player
{
    public class CurrencyUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI goldText;
        
        private CurrencyComponent currencyComponent;

        private void Start()
        {
            // Find and assign the Text_gold_amount if not already assigned
            if (goldText == null)
            {
                GameObject startGameCanvas = GameObject.Find("StartGameCanvas");
                if (startGameCanvas != null)
                {
                    Transform goldTextTransform = startGameCanvas.transform.Find("Text_gold_amount");
                    if (goldTextTransform == null)
                    {
                        // Try to search deeper in the hierarchy
                        goldTextTransform = FindChildRecursively(startGameCanvas.transform, "Text_gold_amount");
                    }
                    
                    if (goldTextTransform != null)
                    {
                        goldText = goldTextTransform.GetComponent<TextMeshProUGUI>();
                        Debug.Log("Found and assigned Text_gold_amount");
                    }
                    else
                    {
                        Debug.LogError("Could not find Text_gold_amount in StartGameCanvas hierarchy");
                    }
                }
                else
                {
                    Debug.LogError("StartGameCanvas not found in scene");
                }
            }
            
            currencyComponent = FindFirstObjectByType<CurrencyComponent>();
            
            if (currencyComponent != null)
            {
                currencyComponent.OnGoldChanged += UpdateGoldText;
                UpdateGoldText(currencyComponent.CurrentGold);
            }
            else
            {
                Debug.LogWarning("No CurrencyComponent found in scene!");
            }
        }

        private void UpdateGoldText(int goldAmount)
        {
            if (goldText != null)
            {
                goldText.text = goldAmount.ToString();
            }
        }

        // Helper method to find a child recursively in the hierarchy
        private Transform FindChildRecursively(Transform parent, string childName)
        {
            foreach (Transform child in parent)
            {
                if (child.name == childName)
                {
                    return child;
                }
                
                Transform found = FindChildRecursively(child, childName);
                if (found != null)
                {
                    return found;
                }
            }
            
            return null;
        }
    }
}
