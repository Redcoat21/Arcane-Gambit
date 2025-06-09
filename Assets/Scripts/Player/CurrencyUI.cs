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
    }
}
