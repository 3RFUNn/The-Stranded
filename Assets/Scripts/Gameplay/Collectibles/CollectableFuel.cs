using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class CollectableFuel : ItemCollectible
{
    public EnergyCharger EnergyCharger;
    public int EnergyValue = 10;
    public static int fuelCount = 0; // Tracks the total number of collected fuel items
    public int maxFuel = 8; // Maximum limit of collectible fuel
    public Text fuelCounterText; // Reference to the UI Text element for fuel count

    private void Awake()
    {
        EnergyCharger = GameObject.Find("EnergyCharger").GetComponent<EnergyCharger>();
        PromptUIText = "Collect Fuel";

        // Locate the fuel counter UI if not set in the Inspector
        if (fuelCounterText == null)
        {
            fuelCounterText = GameObject.Find("FuelCounterText").GetComponent<Text>();
        }

        UpdateFuelCounterUI();
    }

    public override void Interact()
    {
        if (fuelCount < maxFuel)
        {
            fuelCount++;
            EnergyCharger.EnergyInventory += EnergyValue;
            PlaySound();
            ShowFloaterMessage();
            UpdateFuelCounterUI();
            Destroy(gameObject);
        }
        else
        {
            //can 
            Debug.Log("Fuel inventory is full!");
        }
    }

    private void UpdateFuelCounterUI()
    {
        if (fuelCounterText != null)
        {
            fuelCounterText.text = $"{fuelCount}/{maxFuel}";
        }
        else
        {
            Debug.LogWarning("Fuel counter UI is not assigned!");
        }
    }
}
