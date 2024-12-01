using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class GamePhaseManager : MonoBehaviour
{
    public static GamePhaseManager instance;

    public Animator IntroCameraAnimator;
    public GameObject Player;
    public Camera MainCamera, CutSceneCamera;
    public GameObject HUD, MainMenu, PauseMenu;
    public PlayerInput Input;
    public EnemySpawner EnemySpawner;
    public GameObject playerCamera;
    public PlayerTablet PlayerTablet = null;

    [Header("-----------SETTINGS------------")]
    public bool EnableIntro;
    public bool EnableEnemy;
    public bool StartWithTablet = false;

    private GameObject _currentUI;


    private void Awake() {
        instance = this;
    }

    void Start()
    {
        if (EnableIntro) {
            Player.SetActive(false);
            MainMenu.SetActive(true);
        } else{
            //hide main menu and directly start game
            MainMenu.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        if (StartWithTablet){
            PlayerTablet.gameObject.SetActive(true);
            PlayerTablet.SetMessage(0);
        }

        EnemySpawner.enabled = EnableEnemy ? true : false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    //this is called when start button in main menu is clicked
    public void StartGame(){
        //switch to intro cutscene camera
        MainCamera.enabled = false;
        CutSceneCamera.enabled = true;

        //hide UI
        HUD.SetActive(false);
        MainMenu.SetActive(false);

        //hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        //disable all input
        Input.enabled = false;

        //play intro cutscene
        IntroCameraAnimator.Play("IntroCamera");
    }

    public void afterIntro(){
        //align player's camera position with intro cutscene camera 
        Player.transform.position = CutSceneCamera.transform.position;
        MainCamera.transform.rotation = CutSceneCamera.transform.rotation;

        //switch to player's camera
        CutSceneCamera.transform.parent.gameObject.SetActive(false);
        MainCamera.enabled = true;
        
        //show player and UI
        HUD.SetActive(true);
        Player.SetActive(true);

        //enable input
        Input.enabled = true;
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
}
