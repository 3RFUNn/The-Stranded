using Cinemachine;
using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Playables;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager instance;
    
    public Difficulty difficultyManager;

    public Animator IntroCameraAnimator;
    public GameObject Player;
    public GameObject HUD, MainMenu, PauseMenu;
    public PlayerInput Input;
    public PlayerTablet PlayerTablet = null;
    public PlayableDirector Director;
    public CinemachineBrain CineBrain;
    public CinemachineVirtualCamera FpsVCam;
    public GameObject CutSceneGO;
    public AudioSource BGM;
    public Transform EndingPos;
    public GameObject FPSHandCam;
    public GunSystem gunSystem;
    
    public InventoryManager inventoryManager;

    public EnemySpawnerWithIndividualRange enemySpawner;
    
    public int difficulty = 0;

    [Header("-----------SETTINGS------------")]
    public bool EnableIntro;
    public bool EnableEnemy;
    public bool StartWithTablet = false;
    public bool StartWithFullFuel = false;

    public AudioSource GenericAudioSource;

    public AudioSource BackgroundAudioSource;

    public AudioClip BackgroundMusicClip;

    public AudioClip EnemyDieClip;

    public GameObject FloaterText;

    public Transform FloaterMessageSpawnPoint;

    private GameObject _currentUI;

    public FuelCounter fuelCounter;

    public EnergyCharger EnergyCharger;

    public GameObject characterMesh;



    private void Awake() {
        instance = this;
        inventoryManager = InventoryManager.instance;
        
    }

    void Start()
    {
#if !UNITY_EDITOR
        EnableIntro = true;
        StartWithTablet = false;
        StartWithFullFuel = false;
#endif
        if (EnableIntro) {
            ShowAndUnLockCursor();
            //switch to mainmenu VCam
            FpsVCam.Priority = -1;
            //switch input action map to UI
            Input.SwitchCurrentActionMap("UI");
            //hide HUD
            HUD.SetActive(false);
            //show main menu and cutscene GOs
            MainMenu.SetActive(true);
            CutSceneGO.SetActive(true);
            //play audiosource on main camera
            BGM.Play();
            enemySpawner.InitiateEnemies();
            ApplySavedDifficulty();
            characterMesh.SetActive(false);
        } else{
            //hide main menu and directly start game
            MainMenu.SetActive(false);
            HideAndLockCursor();
            HUD.SetActive(true);
            PlayPostCutSceneBGM();
            enemySpawner.InitiateEnemies();
            ApplySavedDifficulty();
            characterMesh.SetActive(true);
        }

        if (StartWithTablet){
            PlayerTablet.gameObject.SetActive(true);
            PlayerTablet.SetMessage(0);
        }

        if (StartWithFullFuel){
           EnergyCharger.EnergyInventory = 100;
        }
    }

    private void PlayPostCutSceneBGM(){
        if (BackgroundAudioSource != null && BackgroundMusicClip != null)
        {
            // Set the clip to the audio source
            BackgroundAudioSource.clip = BackgroundMusicClip;
            
            // Enable looping
            BackgroundAudioSource.loop = true;
            
            // Play the audio
            BackgroundAudioSource.Play();
        }
    }

    //this is called when start button in main menu is clicked
    public void StartGame(){
        HideAndLockCursor();    

        //hide HUD and disable input
        SwitchCutscene(true);

        //play intro cutscene
        Director.Play();

        //fade out music
        BGM.DOFade(0f, 6f).OnComplete(() =>
        {
            BGM.Stop();
            BGM.volume = 1f;
        });
    }

    public void AfterIntro(){
        //show fps camera  
        FpsVCam.Priority = 13;
        
        inventoryManager.isMainMenu = false;

        //show HUD and enable input
        SwitchCutscene(false);

        enemySpawner.InitiateEnemies();
        ApplySavedDifficulty();
        PlayPostCutSceneBGM();

        //hide cutscene objects
        CutSceneGO.SetActive(false);

        //switch input action map
        Input.SwitchCurrentActionMap("Player");

    }

    //freeze time, display cursor, disable in-game inputs
    public void Pause(){
        Time.timeScale = 0;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        Input.SwitchCurrentActionMap("UI");
    }

    //unfreeze time, hide cursor, enable in-game inputs
    public void Resume(){
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        Input.SwitchCurrentActionMap("Player");
    }

    //pause and display ui
    public void Pause(GameObject ui){
        ui.SetActive(true);
        _currentUI = ui;
        Pause();
    }

    //resume and hide ui
    public void Resume(GameObject ui){
        ui.SetActive(false);
        _currentUI = null;
        Resume();
    }

    public void OnPause(InputAction.CallbackContext ctx){
        //only open pause menu during playing
        if (!MainMenu.activeSelf && ctx.started) {
            Pause(PauseMenu);
        }
    }
    public void OnResume(InputAction.CallbackContext ctx) {
        if (_currentUI != null && ctx.performed) {
            Resume(_currentUI);
        }
    }

    public void HideAndLockCursor(){
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowAndUnLockCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    //handle HUD and input availability when enter/exit a cutscene
    public void SwitchCutscene(bool isInCutscene){
        if (isInCutscene) {
            HUD.SetActive(false);
            characterMesh.SetActive(false);
            Input.enabled = false;
        }
        else { 
            HUD.SetActive(true);
            Input.enabled = true;
            enemySpawner.InitiateEnemies();
            ApplySavedDifficulty();
            PlayPostCutSceneBGM();
            characterMesh.SetActive(true);
        }
    }

    //Go to the ending of the game based on ending conditions 
    public void ToEnding(){
        Player.transform.position = EndingPos.position;
        Player.transform.LookAt(EndingPos.position + EndingPos.right);
    }
    
    
    private void ApplySavedDifficulty()
    {
        int savedDifficulty = PlayerPrefs.GetInt("Difficulty", 0); // Default to easy if not set
        switch (savedDifficulty)
        {
            case 0:
                difficultyManager.ApplyEasySettings();
                break;
            case 1:
                difficultyManager.ApplyMediumSettings();
                break;
            case 2:
                difficultyManager.ApplyHardSettings();
                break;
        }
    }
    
}
