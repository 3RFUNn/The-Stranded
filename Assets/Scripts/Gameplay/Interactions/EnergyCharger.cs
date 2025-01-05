using Cinemachine;
using DG.Tweening;
using Gameplay.Interactions;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class EnergyCharger : Interactable
{
    public CinemachineVirtualCamera EndingVCam, RechargingVCam; 
    public SlideDoor EndRoomDoor;

    public int EnergyInventory
    {
        get { 
            return _energyInventory;
        }
        set {
            if (EnergySlider.value <= 100) {
                if (value == 0) {
                    PromptUIText = "No fuel for recharge";
                }
                else {
                    PromptUIText = "Recharge (you have " + value + "% fuel)";
                }
            } else{
                CanInteract = false;
            }
            _energyInventory = value;
        } 
    }
    public Slider EnergySlider;

    [SerializeField]
    private int _energyInventory;

    private void Awake() {
        EnergyInventory = 0;
    }

    public override void Interact() {
        base.Interact();
        if(EnergyInventory != 0){
            RechargingVCam.Priority = 20;
            int remain = (EnergyInventory + (int)EnergySlider.value) % 100;
            DOTween.To(() => EnergySlider.value, x => EnergySlider.value = x, Mathf.Min(EnergySlider.value + EnergyInventory, 100), 1.5f)
                .SetEase(Ease.OutQuint)
                .OnComplete(()=>{
                    if (EnergySlider.value + EnergyInventory > 100f) {
                        UnlockEnding();
                    }
                    RechargingVCam.Priority = 10;
                });
            EnergyInventory = remain;
        }
    }

    public async void UnlockEnding(){
        CanInteract = false;
        GamePhaseManager.instance.SwitchCutscene(true);
        Time.timeScale = 0f;
        await Task.Delay(500);

        Time.timeScale = 1f;
        EndingVCam.Priority = 20;
        await Task.Delay(1000);
        Time.timeScale = 0f;

        EndRoomDoor.Open();

        await Task.Delay((int)EndRoomDoor.MoveDuration * 1000 + 1000);

        EndingVCam.Priority = 10;
        GamePhaseManager.instance.SwitchCutscene(false);
        Time.timeScale = 1f;
    }
}
