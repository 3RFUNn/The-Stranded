using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Cinemachine;
using System.Threading.Tasks;

public class CollectablesCounter : MonoBehaviour{
    private int requiredFuel = 8;
    private int requiredMessage = 7;

    public int currentFuel = -1;
    public int currentMessage = -1;

    public CinemachineVirtualCamera ShipDoorVCam;
    public LayerMask SpaceshipInterior;
    public SlideDoor Door;

    [SerializeField] TextMeshProUGUI FuelCountText;
    [SerializeField] TextMeshProUGUI MessageCountText;

    void Start() {
        UpdateFuel();
        UpdateMessage();
    }

    public void UpdateFuel(){
        currentFuel++;
        FuelCountText.text = $"{currentFuel}/{requiredFuel}";
    }

    public async void UpdateMessage(){
        currentMessage++;
        MessageCountText.text = $"{currentMessage}/{requiredMessage}";
        if (currentMessage == requiredMessage){
            GamePhaseManager.instance.SwitchCutscene(true);
            Time.timeScale = 0f;
            await Task.Delay(500);

            LayerMask mask = Camera.main.cullingMask;
            Camera.main.cullingMask = SpaceshipInterior;
            ShipDoorVCam.Priority = 20;
            await Task.Delay(1000);

            Door.Open();
            await Task.Delay((int)Door.MoveDuration * 1000 + 1000);

            Camera.main.cullingMask = mask;
            ShipDoorVCam.Priority = 10;
            GamePhaseManager.instance.SwitchCutscene(false);
            Time.timeScale = 1f;
        }
    }
}
