using System;
using UnityEngine;

namespace Components.Mana
{
    /// <summary>
    /// Component that handles the mana of an entity
    /// </summary>
    public class ManaComponent : MonoBehaviour
    {
        [SerializeField]
        [Range(1, 100000)]
        private int maximumMana;

        [SerializeField]
        private int currentMana;

        public int MaximumMana
        {
            get => maximumMana;
            set => maximumMana = value;
        }

        public int CurrentMana
        {
            get => currentMana;
            set => currentMana = value;
        }

        // OnManaChanged is an event that will be triggered when the mana of the object changes
        public event Action<int> OnManaChanged;

        /// <summary>
        /// Use mana from the entity
        /// </summary>
        /// <param name="amount">Amount of mana to use</param>
        public void UseMana(int amount)
        {
            currentMana = Mathf.Max(currentMana - amount, 0);
            OnManaChanged?.Invoke(currentMana);
        }

        /// <summary>
        /// Restore mana to the entity
        /// </summary>
        /// <param name="amount">Amount of mana to restore</param>
        public void RestoreMana(int amount)
        {
            int tempMana = currentMana + amount;
            currentMana = tempMana > maximumMana ? maximumMana : tempMana;
            OnManaChanged?.Invoke(currentMana);
        }
    }
}
