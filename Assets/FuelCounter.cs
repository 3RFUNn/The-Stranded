using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class FuelCounter : MonoBehaviour{
    private int requiredFuel = 8;

    public int currentFuel = -1;

    [SerializeField] TextMeshProUGUI Text;

    void Start() {
        UpdateFuel();
    }

    public void UpdateFuel(){
        currentFuel++;
        Text.text = $"{currentFuel}/{requiredFuel}";
    }
}
