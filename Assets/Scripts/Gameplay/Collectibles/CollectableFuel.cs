using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CollectableFuel : ItemCollectible
{
    public EnergyCharger EnergyCharger;
    public int EnergyValue = 10;

    private void Awake() {
        EnergyCharger = FindFirstObjectByType<EnergyCharger>();
        PromptUIText = "Collect Fuel";
    }

    public override void Interact() {
        EnergyCharger.EnergyInventory += EnergyValue;
        PlaySound();
        ShowFloaterMessage();
        UpdateFuel();
        Destroy(gameObject);
    }

    private void UpdateFuel(){
        GamePhaseManager.instance.fuelCounter.UpdateFuel();
    }
}
