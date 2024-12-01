using Gameplay.Interactions;
using Ink.Runtime;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class Talkable : Interactable
{
    public string NPCName;
    public float DisplayInterval;
    public Story Story;
    public GameObject DialogueUI;
    public RectTransform OptionsParent;
    public TextMeshProUGUI DialogueTextUI;
    public TextAsset InkJSONAsset;
    public GameObject CloseButton, NextButton;

    public Button OptionPrefab = null;

    private string _currentDialogue;
    private Coroutine _displayCoroutine;
    private bool _shouldShowImmediately = false;

    private IEnumerator DisplayDialogue(string text) {
        if (DialogueTextUI.text != null) {
            DialogueTextUI.text = "";
        }
        for (int i = 0; i < text.Length; i++) {
            if(_shouldShowImmediately){
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

    private void Awake() {
        promptUIText = $"Talk with {NPCName}";
    }
    void Start()
    {
        DialogueUI.SetActive(false);
    }

    void Initialize(){
        
        
    }
    void StartDialogue(){
        Story = new Story(InkJSONAsset.text);
        DialogueUI.SetActive(true);
        CloseButton.SetActive(false);
        NextButton.SetActive(false);
        RefreshView();
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
            //show our avaliable choices
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

    public override void Interact() {
        GamePhaseManager.instance.Input.SwitchCurrentActionMap("UI");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        ShouldShowPrompt = false;
        GamePhaseManager.instance.Input.actions["Next"].performed += OnNextDialogue;
        
        StartDialogue();
        //_displayCoroutine =  StartCoroutine(DisplayDialogue(_currentDialogue));
    }

    public void OnCloseDialogue(){
        GamePhaseManager.instance.Resume();
        DialogueUI.SetActive(false);
        ShouldShowPrompt = true;
        GamePhaseManager.instance.Input.actions["Next"].performed -= OnNextDialogue;
    }

    public void OnNext() {
        RefreshView();
    }

    public void OnNextDialogue(InputAction.CallbackContext ctx){
        if (_displayCoroutine != null) { 
            _shouldShowImmediately = true;
        }else{
            
        }
    }
}
