using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ExitDoor : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string gameSceneName = "MainScene 2";
    [SerializeField] private string mainMenuSceneName = "MainMenuScene";

    [Header("UI Panels")]
    [SerializeField] private GameObject storyPanel;
    [SerializeField] private GameObject normalEndingPanel;
    [SerializeField] private GameObject trueEndingPanel;

    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private TextMeshProUGUI normalEndingText;
    [SerializeField] private TextMeshProUGUI trueEndingText;
    [SerializeField] private Button continueButton;
    [SerializeField] private Button normalEndingContinueButton;
    [SerializeField] private Button trueEndingContinueButton;

    private bool playerNearby = false;
    private bool storyTriggered = false;

    private readonly int[] storyCheckpointLevels = { 3, 6, 9 };
    private List<string> currentStory;
    private int currentLine = 0;
    private Action onStoryComplete;

    private bool isLevel9EndingPhase = false;

    private void Start()
    {
        storyPanel.SetActive(false);
        normalEndingPanel.SetActive(false);
        trueEndingPanel.SetActive(false);

        continueButton.onClick.AddListener(AdvanceStory);
        normalEndingContinueButton.onClick.AddListener(AdvanceEnding);
        trueEndingContinueButton.onClick.AddListener(AdvanceEnding);
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !storyTriggered)
        {
            if (Array.Exists(storyCheckpointLevels, l => l == LevelManager.LevelCounter))
            {
                storyTriggered = true;
                StartStory();
            }
            else
            {
                ProceedToNextLevel();
            }
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            ProceedToNextLevel(reset: false);
        }
    }

    private void StartStory()
    {
        currentLine = 0;
        currentStory = GetStoryForLevel(LevelManager.LevelCounter);

        if (currentStory != null && currentStory.Count > 0)
        {
            storyPanel.SetActive(true);
            storyText.text = currentStory[0];
        }
        else
        {
            ProceedToNextLevel(); // fallback
        }
    }

    private void AdvanceStory()
    {
        currentLine++;

        if (currentLine < currentStory.Count)
        {
            storyText.text = currentStory[currentLine];
        }
        else
        {
            storyPanel.SetActive(false);

            if (LevelManager.LevelCounter == 9)
            {
                // After level 9 story, show the ending
                isLevel9EndingPhase = true;
                ShowEnding();
            }
            else
            {
                ProceedToNextLevel();
            }
        }
    }

    private void ShowEnding()
    {
        storyPanel.SetActive(false);
        currentLine = 0;

        if (LevelManager.StoryCounter == 6)
        {
            currentStory = GetTrueEndingStory();
            trueEndingPanel.SetActive(true);
            trueEndingText.text = currentStory[0];
        }
        else
        {
            currentStory = GetNormalEndingStory();
            normalEndingPanel.SetActive(true);
            normalEndingText.text = currentStory[0];
        }
    }

    private void AdvanceEnding()
    {
        currentLine++;

        bool isTrue = LevelManager.StoryCounter == 6;

        if (currentLine < currentStory.Count)
        {
            if (isTrue)
                trueEndingText.text = currentStory[currentLine];
            else
                normalEndingText.text = currentStory[currentLine];
        }
        else
        {
            if (isTrue)
                trueEndingPanel.SetActive(false);
            else
                normalEndingPanel.SetActive(false);

            ProceedToNextLevel(reset: true);
        }
    }

    private void ProceedToNextLevel(bool reset = false)
    {
        if (reset || LevelManager.LevelCounter == 9)
        {
            LevelManager.LevelCounter = 1;
            PlayerManager.Reset();
            LevelManager.StoryCounter = 0;
            SceneManager.LoadScene(mainMenuSceneName);
        }
        else
        {
            LevelManager.LevelCounter++;
            SceneManager.LoadScene(gameSceneName);
        }

        Debug.Log("Current Level: " + LevelManager.LevelCounter);
    }

    private List<string> GetStoryForLevel(int level)
    {
        switch (level)
        {
            case 3:
                return new List<string>
                {
                "[Gray clouds hang low over Arcana Cemetery – wind rustles the trees.]",
                "[Muffled footsteps on damp grass.]",
                "[Ethan stands alone at the headstone, hands in his pockets, eyes heavy.]",
                "[The gravesto  ne reads: *Jonathan Reed – Beloved Father. 1959–2025.*]",
                "You: ...Did you know how much I hated you?",
                "[Silence. A crow caws in the distance.]",
                "You: I waited years... hoping you’d say something. Anything.",
                "You: And when you finally did... it was too late.",
                "[He pulls out the letter from his coat, unfolding it slowly.]",
                "You: I read this a hundred times... trying to believe it was real.",
                "You: Proud of me, huh? Funny how I never heard that when it mattered.",
                "[His voice breaks slightly, but he swallows it down.]",
                "You: I’m messed up, Dad. I drink, I yell, I push people away.",
                "You: ...Guess I didn’t fall far from the tree.",
                "[He kneels, placing the letter gently at the base of the stone.]",
                "You: I don’t know if I can forgive you. But I think... I want to try.",
                "[A long pause. The wind grows still.]",
                "You: Goodbye, Dad.",
                "[Ethan stands, eyes closed for a moment, before walking away—slowly, but not aimlessly.]"
                };
            case 6:
                return new List<string>
                {
                "[Office – quiet, tense. A message pops up on Ethan’s screen: *“Come to my office.”*]",
                "[Ethan exhales, grabs his badge, and stands up slowly.]",
                "[Footsteps echo down the hallway. He knocks once, then enters.]",
                "Boss: Sit down.",
                "[Ethan lowers himself into the chair, eyes downcast.]",
                "Boss: You know why you're here.",
                "You: Yeah.",
                "Boss: You’ve missed deadlines. You've shown up late. You’ve been unreliable.",
                "You: I know.",
                "Boss: I gave you chances. More than I should have.",
                "You: I don’t have an excuse.",
                "[The boss watches him in silence for a moment, then sighs.]",
                "Boss: Ethan... I'm letting you go.",
                "[Ethan nods slowly. No fight left in him.]",
                "You: Understood.",
                "[Cut to: Ethan packing his desk into a cardboard box. A photo frame. A few notebooks. A coffee mug. A worn-out pen.]",
                "[Coworkers glance over, but no one speaks.]",
                "[He takes one last look at the empty desk before walking toward the exit.]",
                "[Door closes behind him. A cold breeze greets him outside.]",
                "You: ...That’s that."
                };
            case 9:
                return new List<string>
                {
                "[Front door creaks open – afternoon light spills into the hallway.]",
                "[Ethan turns the corner. His wife stands at the door, their daughter clutching her hand, bags at their feet.]",
                "[Silence. Heavy. Final.]",
                "Wife: We’re leaving, Ethan.",
                "[Ethan says nothing – just stares, hollowed out.]",
                "Wife: I waited. I tried. But I can’t raise her in this… in *you* anymore.",
                "[Their daughter looks up at him, eyes confused but quiet.]",
                "You: ...I know.",
                "[Wife's expression tightens – not with anger, but heartbreak.]",
                "Wife: You could’ve fought for us. But you didn’t.",
                "You: I didn’t know how.",
                "Wife: Then learn. Before you lose more than just us.",
                "[She turns, opening the door. Their daughter hesitates, looking back at him.]",
                "Daughter: Bye, Daddy.",
                "[Ethan kneels down, forcing a weak smile.]",
                "You: Bye, sweetheart...",
                "[They walk out. The door closes slowly, the final click echoing in the empty home.]",
                "[Ethan remains still, staring at the place they once stood.]",
                "You: ...Too late."
                };
            default:
                return null;
        }
    }

    private List<string> GetNormalEndingStory()
    {
        return new List<string>
        {
        "[Darkness surrounds him – flickering lights and echoes of distant voices.]",
        "[He walks forward, unsure, familiar scenes rising like mist.]",
        "You: Haven’t I been here before…?",
        "[A hallway stretches endlessly – his office, his house, his father’s grave all bleed into one.]",
        "You: No... this can't be real...",
        "[He opens a door – the same argument, the same silence, the same goodbye.]",
        "You: Why does it keep happening?",
        "[A child’s voice calls faintly from the distance.]",
        "Daughter: Daddy... are you coming home?",
        "[He runs toward it – but the hallway shifts again. Back to the start.]",
        "[Phone vibrating...]",
        "Sister: Hey... I didn't know how to say this...",  
        "[Ethan freezes. His breath catches in his throat.]",
        "You: ...Wait.",
        "[Everything fades to black. The loop begins anew.]"
        };
    }

    private List<string> GetTrueEndingStory()
    {
        return new List<string>
        {
        "[White light. Steady beeping. The world is quiet for once.]",
        "[Ethan opens his eyes slowly – a hospital ceiling stares back.]",
        "[His body aches. Tubes. Machines. A nurse gasps.]",
        "Nurse: He's awake! Get the doctor!",
        "[Ethan turns his head, tears already forming.]",
        "You: I'm... alive?",
        "[Memories rush in – the crash, the drink, the fights, the silence.]",
        "You: I did this... I really did this.",
        "[A moment of heavy silence. Then the door opens – his wife and daughter enter, uncertain.]",
        "[He reaches out, hand trembling.]",
        "You: I want to fix this. If you’ll let me.",
        "[Tears fall from her eyes – a single nod. Their daughter clutches his hand tightly.]",
        "Daughter: Daddy...",
        "You: I’m here now. I promise.",
        "[The monitor beeps steady and strong as the morning sun breaks through the window.]",
        };
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
