using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StoryPoint : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject storyDialogPanel;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Button continueButton;

    [Header("Story Lines")]
    [TextArea(2, 4)]
    [SerializeField] private List<string> storyLines;

    private int currentLine = 0;
    private bool playerNearby = false;
    private bool isDialogActive = false;

    private void Start()
    {
        storyDialogPanel.SetActive(false);
        continueButton.onClick.AddListener(AdvanceStory);
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !isDialogActive)
        {
            StartStory();
        }
    }

    private void StartStory()
    {
        currentLine = 0;
        isDialogActive = true;
        storyDialogPanel.SetActive(true);
        storyText.text = storyLines[currentLine];
    }

    private void AdvanceStory()
    {
        currentLine++;

        if (currentLine < storyLines.Count)
        {
            storyText.text = storyLines[currentLine];
        }
        else
        {
            EndStory();
        }
    }

    private void EndStory()
    {
        isDialogActive = false;
        storyDialogPanel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerNearby = false;
        }
    }
}
