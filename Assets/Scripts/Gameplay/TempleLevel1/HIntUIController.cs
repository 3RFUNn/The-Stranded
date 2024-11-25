using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintUIController : MonoBehaviour
{
    [SerializeField] private GameObject HintUIPanel; // Reference to the Hint UI Panel
    [SerializeField] private GameObject HintText;

    void Start()
    {
        // Ensure the HintUIPanel is disabled at the start if it is not already
        if (HintUIPanel != null)
        {
            Debug.Log("Text disabled - start");
            HintText.SetActive(false);
            HintUIPanel.SetActive(true);
        }
        else
        {
            Debug.LogError("HintUIPanel is not assigned. Please assign it in the Inspector.");
        }
    }

    void Update()
    {
        if(AppHelper.gameEnded){
         HintText.SetActive(false);   
        }
        // Check if the H key is pressed
        if (Input.GetKeyDown(KeyCode.H) && !AppHelper.gameEnded)
        {
            ToggleHintPanel();
            Debug.Log("Hint text active state "+ !HintText.activeSelf);
            
        }
    }

    private void ToggleHintPanel()
    {
        if (HintUIPanel != null)
        {
            // Toggle the active state of the HintUIPanel
            HintUIPanel.SetActive(!HintUIPanel.activeSelf);
            HintText.SetActive(!HintUIPanel.activeSelf);
            if(!AppHelper.hintDisplayed){
                AppHelper.hintDisplayed = true;
                Debug.Log("Text enabled - Firt time");
                HintText.SetActive(true);
            }
        }
    }
}
