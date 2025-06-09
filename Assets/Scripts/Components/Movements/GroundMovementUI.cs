using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Components.Movements;

public class GroundMovementUI : MonoBehaviour
{
    [SerializeField] private GroundMovementComponent groundMovementComponent;
    [SerializeField] private TextMeshProUGUI movementText;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        if (groundMovementComponent != null)
        {
            UpdateUI();
            groundMovementComponent.OnMovementSpeedChanged += HandleMovementSpeedChanged;
        }
    }
    private void OnDestroy()
    {
        if (groundMovementComponent != null)
        {
            groundMovementComponent.OnMovementSpeedChanged -= HandleMovementSpeedChanged;
        }
    }
    
    private void HandleMovementSpeedChanged(float newSpeed)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        movementText.text = $"Movement Speed: {groundMovementComponent.MoveSpeed}";

        Debug.Log("Base Speed FROM UI: " + groundMovementComponent.MoveSpeed);
    }

}
