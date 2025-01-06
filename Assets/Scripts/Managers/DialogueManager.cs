using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class DialogueSystem : MonoBehaviour
{
    public static DialogueSystem instance;

    public float DisplayInterval;
    public Story Story;
    public GameObject DialogueUI;
    public RectTransform OptionsParent;
    public TextMeshProUGUI DialogueTextUI;
    public GameObject CloseButton, NextButton;
    public Button OptionPrefab = null;
    public GameObject HUD;

    private string _currentDialogue;
    private Talkable _currentTalkable;
    private Coroutine _displayCoroutine;
    private bool _shouldShowImmediately = false;
    
    public AudioSource audioSource;
    public AudioClip monsterRoarClip;
    


    [SerializeField] private GameObject alien;
    [SerializeField] private GameObject boss;

    [SerializeField] private GameObject[] magazine;

    private void Awake() {
        instance = this;
    }

    void Start() {
        DialogueUI.SetActive(false);
    }

    private IEnumerator DisplayDialogue(string text) {
        if (DialogueTextUI.text != null) {
            DialogueTextUI.text = "";
        }
        for (int i = 0; i < text.Length; i++) {
            if (_shouldShowImmediately) {
                while (i < text.Length) {
                    DialogueTextUI.text += text[i];
                    i++;
                }
                _shouldShowImmediately = false;
                break;
            }
            DialogueTextUI.text += text[i];
            yield return new WaitForSeconds(DisplayInterval);
        }
    }

    public void StartDialogue(TextAsset inkJSON, Talkable talkable) {
        _currentTalkable = talkable;
        Story = new Story(inkJSON.text);
        
        // Bind the external functions when starting dialogue
        BindStoryFunctions();
        
        DialogueUI.SetActive(true);
        CloseButton.SetActive(false);
        NextButton.SetActive(false);
        HUD.SetActive(false);
        RefreshView();
    }

    private void BindStoryFunctions()
    {
        Story.BindExternalFunction("JoinStranded", () => {
            Debug.Log("Player chose to join the Stranded");
            OnJoinStranded();
        });

        Story.BindExternalFunction("FounderBetrayal", () => {
            Debug.Log("Founder betrays the player");
            OnFounderBetrayal();
        });
    }

    // Function called when player joins the Stranded
   private async void OnJoinStranded()
{
    // Add your game logic here for joining the Stranded
    if (_currentTalkable != null)
    {
        OnCloseDialogue();
        await Task.Delay(1000);
        UnityEngine.SceneManagement.SceneManager.LoadScene(2);
    }
}

    // Function called when the founder betrays the player
    private async void OnFounderBetrayal()
    {
        // Add your game logic here for the betrayal sequence
        if (_currentTalkable != null)
        {
            alien.SetActive(false);
            OnCloseDialogue();
            
            await Task.Delay(1000);

            // Play the monster roaring sound
            audioSource.PlayOneShot(monsterRoarClip);

            // Wait for 3 seconds
            await Task.Delay(2000);

            // Play the boss fight music
            
            audioSource.Play();

            boss.SetActive(true);
            magazine[0].SetActive(true);
            magazine[1].SetActive(true);
        }
    }

    void RefreshView() {
        // Remove all the texts on screen
        RemoveChildren();

        if (Story.canContinue) {
            //NPC dialogue is continuing
            //gets the next line of the story
            string text = Story.Continue();

            // Display the text on screen.
            DialogueTextUI.text = text;
            NextButton.SetActive(true);
        }
        
        if (Story.currentChoices.Count > 0) {
            //show our available choices
            NextButton.SetActive(false);
            for (int i = 0; i < Story.currentChoices.Count; i++) {
                Choice choice = Story.currentChoices[i];
                Button button = CreateChoiceView(choice.text.Trim(), OptionsParent);
                // Tell the button what to do when we press it
                button.onClick.AddListener(delegate
                {
                    OnClickChoiceButton(choice);
                });
            }
        }
        else if (!Story.canContinue) {
            //the whole dialogue is ended, show the close button
            NextButton.SetActive(false);
            CloseButton.SetActive(true);
        }
    }

    // When we click the choice button, tell the story to choose that choice
    void OnClickChoiceButton(Choice choice) {
        Story.ChooseChoiceIndex(choice.index);
        RefreshView();
    }

    // Creates a button showing the choice text
    Button CreateChoiceView(string text, Transform parent) {
        // Creates the button from a prefab
        Button choice = Instantiate(OptionPrefab, parent);

        // Gets the text from the button prefab
        TextMeshProUGUI choiceText = choice.GetComponentInChildren<TextMeshProUGUI>();
        choiceText.text = text;

        return choice;
    }

    // clear all the texts
    void RemoveChildren() {
        //destroy options buttons
        int childCount = OptionsParent.childCount;
        for (int i = childCount - 1; i >= 0; --i) {
            Destroy(OptionsParent.GetChild(i).gameObject);
        }
        //clear NPC dialogue
        DialogueTextUI.text = "";
    }

    public void OnCloseDialogue() {
        GamePhaseManager.instance.Resume();
        DialogueUI.SetActive(false);
        HUD.SetActive(true);
        _currentTalkable.CanInteract = true;
    }

    public void OnNext() {
        RefreshView();
    }

    public void OnNextDialogue(InputAction.CallbackContext ctx) {
        if (_displayCoroutine != null) {
            _shouldShowImmediately = true;
        }
    }
}