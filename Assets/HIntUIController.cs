using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HintUIController : MonoBehaviour
{
    [SerializeField] private GameObject HintUIPanel; // Reference to the Hint UI Panel

    void Start()
    {
        // Ensure the HintUIPanel is disabled at the start if it is not already
        if (HintUIPanel != null)
        {
            HintUIPanel.SetActive(false);
        }
        else
        {
            Debug.LogError("HintUIPanel is not assigned. Please assign it in the Inspector.");
        }
    }

    void Update()
    {
        // Check if the H key is pressed
        if (Input.GetKeyDown(KeyCode.H))
        {
            ToggleHintPanel();
        }
    }

    private void ToggleHintPanel()
    {
        if (HintUIPanel != null)
        {
            // Toggle the active state of the HintUIPanel
            HintUIPanel.SetActive(!HintUIPanel.activeSelf);
        }
    }
}
