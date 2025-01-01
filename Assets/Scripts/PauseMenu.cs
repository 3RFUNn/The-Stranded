using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    [SerializedField] GameObject pauseMenu;

    void Update()
    {
        if (InputGroup.GetKeyDown(KeyCode.P))
        {
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
        PauseMenu.SetActive(true);
    }

    public void Restart()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
        
    }
    public void Resume()
    {
        Time.timeScale = 1;
        UnityEngine.ScreneManagement.SceneManager.LoadScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());

    }

}
