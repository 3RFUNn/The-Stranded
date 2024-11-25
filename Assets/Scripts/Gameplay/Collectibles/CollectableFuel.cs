using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CollectableFuel : ItemCollectible
{
    public EnergyCharger EnergyCharger;
    public int EnergyValue = 10;

    private void Awake() {
        EnergyCharger = GameObject.Find("EnergyCharger").GetComponent<EnergyCharger>();
        promptUIText = "Collect Fuel";
    }

    public override void Interact() {
        EnergyCharger.EnergyInventory += EnergyValue;
        Destroy(gameObject);
    }
}
