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
    public TextAsset InkJSONAsset;

    private void Awake() {
        PromptUIText = $"Talk with {NPCName}";
    }
 
    public override void Interact() {
        GamePhaseManager.instance.Input.SwitchCurrentActionMap("UI");
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        CanInteract = false;
        //GamePhaseManager.instance.Input.actions["Next"].performed += OnNextDialogue;
        
        DialogueSystem.instance.StartDialogue(InkJSONAsset, this);
        //_displayCoroutine =  StartCoroutine(DisplayDialogue(_currentDialogue));
    }
}
