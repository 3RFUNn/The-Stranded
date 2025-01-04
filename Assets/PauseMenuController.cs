using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;

public class PauseMenuController : MonoBehaviour
{
    public GameObject pauseMenuPanel;
    public Button resumeButton;
    public Button restartButton;
    public Button settingsButton;

    private bool isPaused = false;

    private Button currentSelectedButton;

    void Start()
    {
        pauseMenuPanel.SetActive(false);

        resumeButton.onClick.AddListener(ResumeGame);
        restartButton.onClick.AddListener(RestartGame);
        settingsButton.onClick.AddListener(OpenSettings);

        currentSelectedButton = resumeButton;
        UpdateButtonVisuals();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (isPaused)
        {
            if (Input.GetKeyDown(KeyCode.UpArrow))
            {
                SelectPreviousButton();
            }
            if (Input.GetKeyDown(KeyCode.DownArrow))
            {
                SelectNextButton();
            }

            if (Input.GetKeyDown(KeyCode.Return))
            {
                currentSelectedButton.onClick.Invoke();
            }
        }
    }

    void PauseGame()
    {
        Time.timeScale = 0f;
        pauseMenuPanel.SetActive(true);
        isPaused = true;
        EventSystem.current.SetSelectedGameObject(currentSelectedButton.gameObject);
    }

    void ResumeGame()
    {
        Time.timeScale = 1f;
        pauseMenuPanel.SetActive(false);
        isPaused = false;
    }

    void RestartGame()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    void OpenSettings()
    {
        Debug.Log("Opening Settings");

    }

    void SelectNextButton()
    {
        if (currentSelectedButton == restartButton)
            currentSelectedButton = resumeButton;
        else if (currentSelectedButton == resumeButton)
            currentSelectedButton = settingsButton;
        else if (currentSelectedButton == settingsButton)
            currentSelectedButton = restartButton;

        UpdateButtonVisuals();
        EventSystem.current.SetSelectedGameObject(currentSelectedButton.gameObject);
    }

    void SelectPreviousButton()
    {
        if (currentSelectedButton == settingsButton)
            currentSelectedButton = resumeButton;
        else if (currentSelectedButton == resumeButton)
            currentSelectedButton = restartButton;
        else if (currentSelectedButton == restartButton)
            currentSelectedButton = settingsButton;

        UpdateButtonVisuals();
        EventSystem.current.SetSelectedGameObject(currentSelectedButton.gameObject);
    }

    void UpdateButtonVisuals()
    {
        resumeButton.transform.localScale = Vector3.one;
        restartButton.transform.localScale = Vector3.one;
        settingsButton.transform.localScale = Vector3.one;

        currentSelectedButton.transform.localScale = new Vector3(1.2f, 1.2f, 1f);
    }
}
