using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

[System.Serializable]
public class StoryLinesSet
{
    public List<string> lines = new List<string>();
}

public class StoryPoint : MonoBehaviour
{
    private List<StoryLinesSet> storyLineSets = new List<StoryLinesSet>();
    [Header("UI References")]
    [SerializeField] private GameObject storyDialogPanel;
    [SerializeField] private TextMeshProUGUI storyText;
    [SerializeField] private Button continueButton;

    private int currentLine = 0;
    private bool playerNearby = false;
    private bool isDialogActive = false;
    private bool hasShownThisLevel = false;
    private List<string> currentStoryLines;

    // Valid levels that contain a story
    private readonly int[] storyLevels = { 1, 2, 4, 5, 7, 8 };

    private void Start()
    {
        storyDialogPanel.SetActive(false);
        continueButton.onClick.AddListener(AdvanceStory);

        // Define all 6 story sets
        storyLineSets = new List<StoryLinesSet>
        {
            new StoryLinesSet
            {
                lines = new List<string>
                {
                    "[Phone vibrating...]",
                    "Sister: Hey... I didn't know how to say this...",
                    "Sister: Dad passed away this morning.",
                    "You: ...",
                    "Sister: Hello?",
                    "You: ...",
                    "Sister: I know he wasn't there for many things.",
                    "Sister: But could you atleast attend his funeral!",
                    "You:...",
                    "Sister: *Sigh* Tuesday next week 2 PM, Arcana Funeral.",
                    "[Phone Starts Beeping]",
                    "You: It's always one thing after another..."
                }
            },
            new StoryLinesSet
            {
                lines = new List<string>
                {
                    "[Later that night. A letter sits on the table.]",
                    "[Envelope rustling...]",
                    "You: Huh? What's this...",
                    "[Opens the letter slowly.]",
                    "You: ...",
                    "[Reading the letter silently — it’s from Dad.]",
                    "Dad (Letter): I know I was never the father you needed me to be.",
                    "Dad (Letter): There are things I should have said long ago…",
                    "Dad (Letter): I'm proud of you. Even if I never said it — I always was.",
                    "[Silence. The paper trembles slightly in your hands.]",
                    "You: …No way...",
                    "[You stare blankly at the words — the ones you never expected. The ones he never said in person.]",
                    "You: Why now…?",
                    "[You sink into the chair, gripping the letter tighter.]",
                    "You: Dammit, old man..."
                }
            },
            new StoryLinesSet
            {
                lines = new List<string>
                {
                    "[Office – tense silence, fluorescent lights buzz above.]",
                    "Boss: Are you out of your mind?!",
                    "Boss: The deadline was *yesterday*!",
                    "Boss: What were you even doing this whole week?!",
                    "You: I... I had some personal—",
                    "Boss: I don’t want to hear it!",
                    "Boss: Clients don’t care about your *personal* issues!",
                    "You: I'm sorry, I just—",
                    "Boss: Sorry doesn’t fix the presentation that never got sent!",
                    "Boss: You’ve been slipping. First the absences, now this.",
                    "You: ...",
                    "Boss: Get it together or don’t bother showing up next week.",
                    "[Boss storms out, door slamming shut.]",
                    "[You sit frozen at your desk, staring at the blinking cursor.]",
                    "You: ...One thing after another..."
                }
            },
            new StoryLinesSet
            {
                lines = new List<string>
                {
                    "[Morning at the office – the low hum of chatter and keyboards.]",
                    "[Whispers nearby.]",
                    "Coworker 1: Is he... asleep?",
                    "Coworker 2: Again? That’s the third time this week.",
                    "[Someone approaches quietly.]",
                    "Coworker 1: Hey, wake up... You okay?",
                    "[They pause, noticing the smell.]",
                    "Coworker 2: Is that... alcohol?",
                    "[Your eyes slowly open, unfocused.]",
                    "You: Mm... what time is it...?",
                    "[Coworkers exchange concerned looks.]",
                    "Coworker 1: Look, we’re worried about you. Maybe you should talk to someone.",
                    "You: I’m fine. Just tired.",
                    "[Boss enters the room, expression unreadable.]",
                    "Boss: My office. Now.",
                    "[You glance at your coworkers, who look away, helpless.]",
                    "[You get up slowly, the weight of everything pressing down.]",
                    "You: ...Too late now."
                }
            },
            new StoryLinesSet
            {
                lines = new List<string>
                {
                    "[Living room – voices raised, tension thick in the air.]",
                    "Wife: You said you'd be home by eight!",
                    "Wife: Instead I get a call from a *bartender* at midnight!",
                    "You: I just needed time to think, alright?!",
                    "Wife: Think?! You’re always drinking! You don’t talk, you don’t show up, you don’t even *look* at us anymore!",
                    "You: Don’t turn this around on me!",
                    "Wife: I’m not the one stumbling through the door reeking of whiskey!",
                    "You: I never asked for this life—",
                    "Wife: Then why are you *still here?!*",
                    "[Silence hangs heavy between them.]",
                    "You: ...Because of her.",
                    "[They both glance toward the hallway, where a small figure watches quietly.]",
                    "Wife: She’s the only one holding this together.",
                    "You: I know.",
                    "Wife: Then act like it.",
                    "[The room falls silent again, the damage already done.]"
                }
            },
            new StoryLinesSet
            {
                lines = new List<string>
                {
                    "[Late evening – the dining table is set for two, candles now half-melted. A cold dinner sits untouched.]",
                    "[Front door opens. Footsteps shuffle inside.]",
                    "You: Hey... I’m home.",
                    "[Silence. He looks around, confused.]",
                    "You: Something wrong?",
                    "Wife: Do you even know what day it is?",
                    "[Ethan freezes mid-step. The realization slowly dawns.]",
                    "You: ...Wait, I—",
                    "Wife: Don’t. Just... don’t.",
                    "You: I got caught up at work, I didn’t mean—",
                    "Wife: You forgot. *Again.*",
                    "You: I can fix this, I’ll make it up to you—",
                    "Wife: No, Ethan. You won’t. You never do.",
                    "[She stands up, voice shaking but firm.]",
                    "Wife: This was the last time. I’m done waiting for you to show up.",
                    "You: Please, just—",
                    "Wife: I’ll be at my sister’s. Don’t call.",
                    "[She walks out, door shutting behind her. The silence that follows is deafening.]",
                    "[Ethan stands alone in the dim room, eyes on the table, the candles now barely flickering.]",
                    "You: ...Happy anniversary."
                }
            }
        };

        // Same logic from before
        if (System.Array.Exists(storyLevels, level => level == LevelManager.LevelCounter))
        {
            int storyIndex = GetStoryIndex(LevelManager.LevelCounter);
            if (storyIndex >= 0 && storyIndex < storyLineSets.Count)
            {
                currentStoryLines = storyLineSets[storyIndex].lines;
            }
        }
    }

    private void Update()
    {
        if (playerNearby && Input.GetKeyDown(KeyCode.E) && !isDialogActive && !hasShownThisLevel && currentStoryLines != null)
        {
            StartStory();
        }
    }

    private void StartStory()
    {
        currentLine = 0;
        isDialogActive = true;
        hasShownThisLevel = true;
        storyDialogPanel.SetActive(true);
        storyText.text = currentStoryLines[currentLine];
    }

    private void AdvanceStory()
    {
        currentLine++;

        if (currentLine < currentStoryLines.Count)
        {
            storyText.text = currentStoryLines[currentLine];
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
        LevelManager.StoryCounter++; // Count this as a story completed
    }

    private int GetStoryIndex(int level)
    {
        switch (level)
        {
            case 1: return 0;
            case 2: return 1;
            case 4: return 2;
            case 5: return 3;
            case 7: return 4;
            case 8: return 5;
            default: return -1; // No story for this level
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
