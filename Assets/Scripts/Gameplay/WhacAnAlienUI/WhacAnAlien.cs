
using Cinemachine;
using Gameplay.Interactions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class WhacAnAlien : Interactable
{
    public PlayerInput PlayerInput;
    public List<AlienMove> Aliens;
    public List<GameObject> Doors;
    public TextMeshProUGUI CountDownText, KeyPromptText, DoorOpenText, CrosshairPromptText;
    [Range(0, 20)]
    public float CountDown;
    public float timer;
    public bool Started,isCountingDown;
    public CinemachineVirtualCamera VCam;
    //public WhacAnAlienPuzzleState state;

    void Start()
    {
        Started = false;
        timer = CountDown;
        isCountingDown = false;
    }

    void Update() {
        
    }
    private void FixedUpdate() {
        if (isCountingDown) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                //timeover
                StopAndReset();
                foreach (var alien in Aliens){
                    alien.shouldMoveUp = true;
                }
                ShouldShowPrompt = true;
            }
            else {
                //counting
                CountDownText.text = "Reset Time: " + timer.ToString("F1");
            }
        }
    }
    public override void Interact() {
        if(!isCountingDown){
            //KeyPromptText.enabled = true;
            foreach (var i in Aliens) {
                i.shouldMoveUp = true;
            }
        }
        VCam.Priority = 20;
        GamePhaseManager.instance.FPSHandCam.SetActive(false);
        CrosshairPromptText.enabled = false;
        PlayerInput.SwitchCurrentActionMap("UI");
        ShouldShowPrompt = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void StartCountingDown(){
        GamePhaseManager.instance.FPSHandCam.SetActive(true);
        VCam.Priority = 10;
        PlayerInput.SwitchCurrentActionMap("Player");
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        foreach(var door in Doors){
            door.SetActive(false);
        }

        DoorOpenText.enabled = true;
        CountDownText.enabled = true;
        CrosshairPromptText.enabled = true;

        isCountingDown = true;
    }

    public void StopAndReset(){
        foreach (var door in Doors) {
            door.SetActive(true);
        }

        DoorOpenText.enabled = false;
        //KeyPromptText.enabled = false;
        CountDownText.enabled = false;
        CountDownText.text = null;
        

        timer = CountDown;
        isCountingDown = false;
    }

}
