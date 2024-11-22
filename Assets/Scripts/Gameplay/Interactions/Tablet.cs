using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;

public class Tablet : Interactable
{
    public GameObject uiCanvas;  // The UI canvas that contains the note and button


    void Start()
    {
        // Ensure the UI starts hidden
        uiCanvas.SetActive(false);
    }

    // Override the Interact method from the base class
    public override void Interact()
    {
        ShowNote();
    }

    // This method shows the UI and pauses the game
    void ShowNote()
    {
        GamePhaseManager.instance.Pause(uiCanvas);
    }

    // This method hides the UI and resumes the game
    public void HideNote()
    {
        GamePhaseManager.instance.Resume(uiCanvas);
    }
}
