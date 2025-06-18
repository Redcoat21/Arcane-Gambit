using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitDoor : MonoBehaviour
{
    [SerializeField] private string gameSceneName = "MainScene 2";
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";
    [SerializeField] private float interactionRange = 2f;

    private bool playerNearby = false;

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E))
        {
            if (LevelManager.LevelCounter == 9)
            {
                LevelManager.LevelCounter = 1;
                SceneManager.LoadScene(mainMenuSceneName);
            }
            else
            {
                LevelManager.LevelCounter++;
                SceneManager.LoadScene(gameSceneName);
            }

            Debug.Log("Current Level: " + LevelManager.LevelCounter);
        }
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
