using UnityEngine;

namespace DefaultNamespace
{
    public class Statue : MonoBehaviour
    {
        [SerializeField] private GameObject statueUI;
        [SerializeField] private Player.PlayerCharacter player;
        private InteractableComponent interactableComponent;

        private void Awake()
        {
            interactableComponent ??= GetComponent<InteractableComponent>();
            interactableComponent.OnInteract += () =>
            {
                Debug.Log("Statue interacted with.");
            };
        }
    }
}
