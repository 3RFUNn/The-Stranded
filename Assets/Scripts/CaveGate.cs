using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;
using UnityEngine.SceneManagement;  // Add this for scene management

public class CaveGate : MonoBehaviour
{
    public string nextSceneName;  // The name of the scene to load

    // This method switches to the next scene
    void SwitchScene()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        // Resume the game time in case it was paused
        Time.timeScale = 0;

        // Load the specified scene by its name
        SceneManager.LoadScene(nextSceneName);
        Time.timeScale = 1; 
    }


    private string targetTag = "Player";

    private void OnTriggerEnter(Collider other)
    {
        // Check if the entering GameObject has the specified tag
        if (other.CompareTag(targetTag))
        {
            Debug.Log("Trigger detected with object tagged as: " + targetTag);
            SwitchScene();
            
            // Perform your desired action here
        }
    }

}
