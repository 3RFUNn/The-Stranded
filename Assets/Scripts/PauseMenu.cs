using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public GameObject pauseMenu;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Debug.Log("P key pressed!");
            TogglePauseMenu();
        }
    }

    public void TogglePauseMenu()
    {
        if (Time.timeScale == 0)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }


    public void PauseGame()
    {
        Time.timeScale = 0;
        pauseMenu.SetActive(true);
    }


    public void ResumeGame()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }

    public void RestartGame()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
