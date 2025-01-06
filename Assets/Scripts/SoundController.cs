using UnityEngine;
using UnityEngine.UI;

public class SoundController : MonoBehaviour
{
    public Slider musicSlider;
    public Slider musicSliderMainMenu;
    public float lastSliderValue = 0.15f;

    void Start()
    {
        if (musicSlider != null)
        {
            musicSlider.value = 0.15f;
        }
        if (musicSliderMainMenu != null)
        {
            musicSliderMainMenu.value = 0.15f;
        }
        SetActions();
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            AdjustSliderValue(0.01f);
        }
        if (Input.GetKey(KeyCode.LeftArrow))
        {
            AdjustSliderValue(-0.01f);
        }
    }

    private void SetActions()
    {
        if (musicSlider == null || GamePhaseManager.instance == null || GamePhaseManager.instance.BackgroundAudioSource == null)
        {
            Debug.LogWarning("Music slider or audio source is not set up properly.");
            return;
        }

        // Add listener for both sliders
        musicSlider.onValueChanged.AddListener(CheckAndSetVolume);
        if (musicSliderMainMenu != null)
        {
            musicSliderMainMenu.onValueChanged.AddListener(CheckAndSetVolume);
        }
    }

    private void CheckAndSetVolume(float currentValue)
    {
        if (Mathf.Abs(currentValue - lastSliderValue) > Mathf.Epsilon) // Use Epsilon for float comparison
        {
            GamePhaseManager.instance.BackgroundAudioSource.volume = currentValue;
            GamePhaseManager.instance.BGM.volume = currentValue;

            lastSliderValue = currentValue;
        }
    }

    void AdjustSliderValue(float adjustment)
    {
        if (musicSlider != null)
        {
            musicSlider.value = Mathf.Clamp(musicSlider.value + adjustment, 0f, 1f);
        }
        if (musicSliderMainMenu != null)
        {
            musicSliderMainMenu.value = Mathf.Clamp(musicSliderMainMenu.value + adjustment, 0f, 1f);
        }
    }
}
