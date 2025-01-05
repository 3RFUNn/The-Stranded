using Cinemachine;
using Gameplay.Interactions;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class WhacAnAlien : Interactable
{
    public event Action OnCountdownOver;
    public Action OnCountdownStart;     //will be invoked in AlienMove
    
    public TextMeshProUGUI CountdownText;
    [Range(0, 20)]
    public float CountDown;
    public bool isOneTimeMove = false;  //true if the pillar moves down only once and does not move back
    public CinemachineVirtualCamera VCam;   //switch to this camera when the panel is interacted
    public float MoveDuration = 0.8f;   //time for alien icon and pillar to move up and down

    private bool _isCountingDown = false;
    private float timer;
    void Start()
    {
        timer = CountDown;
    }

    private void FixedUpdate() {
        if (!isOneTimeMove && _isCountingDown) {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                //timeover
                StopAndReset();
            }
            else {
                //counting
                CountdownText.text = "Reset Time: " + timer.ToString("F1");
            }
        }
    }

    public override void Interact() {
        // move camera to preset viewing angle
        VCam.Priority = 20;
        // hide hand rendering and HUD
        GamePhaseManager.instance.FPSHandCam.SetActive(false);
        GamePhaseManager.instance.HUD.SetActive(false);
        // switch input
        GamePhaseManager.instance.Input.SwitchCurrentActionMap("UI");
        // hide prompt
        CanInteract = false;
        // show cursor
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PlaySound();
    }

    public void OnAlienReachedBottom(){
        // return to fps mode
        GamePhaseManager.instance.FPSHandCam.SetActive(true);
        GamePhaseManager.instance.HUD.SetActive(true);
        GamePhaseManager.instance.Resume();
        VCam.Priority = 10;

        if (isOneTimeMove) {
            StopAndReset();
        }else{
            _isCountingDown = true;
            CountdownText.enabled = true;
        }
    }

    public void StopAndReset(){
        if (isOneTimeMove) {
            //the panel cannot be accessed anymore
            CanInteract = false;
        }else{
            CanInteract = true;
            // clear countdown states
            CountdownText.enabled = false;
            CountdownText.text = null;
            timer = CountDown;
            _isCountingDown = false;

            // invoke and clear event
            OnCountdownOver?.Invoke();
        }
        OnCountdownOver = null;
    }
}
