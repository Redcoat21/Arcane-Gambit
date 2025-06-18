using System;
using UnityEngine;

namespace Player
{
    /// <summary>
    /// Component that manages player currency (gold)
    /// </summary>
    public class CurrencyComponent : MonoBehaviour
    {
        [SerializeField]
        private int currentGold = 0;

        public int CurrentGold
        {
            get => currentGold;
            set => currentGold = value;
        }

        private void Start()
        {
            CurrentGold = PlayerManager.Currency;
        }

        // Event that triggers when gold amount changes
        public event Action<int> OnGoldChanged;

        /// <summary>
        /// Add gold to the player's currency
        /// </summary>
        /// <param name="amount">Amount of gold to add</param>
        public void AddGold(int amount)
        {
            if (amount <= 0) return;
            
            currentGold += amount;
            PlayerManager.Currency += amount;
            OnGoldChanged?.Invoke(currentGold);
        }

        /// <summary>
        /// Attempt to spend gold if player has enough
        /// </summary>
        /// <param name="amount">Amount of gold to spend</param>
        /// <returns>True if successful, false if not enough gold</returns>
        public bool SpendGold(int amount)
        {
            if (amount <= 0) return true;
            if (currentGold < amount) return false;
            
            currentGold -= amount;
            PlayerManager.Currency -= amount;
            OnGoldChanged?.Invoke(currentGold);
            return true;
        }
    }
}
