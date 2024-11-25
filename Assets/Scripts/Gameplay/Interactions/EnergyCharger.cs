using DG.Tweening;
using Gameplay.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EnergyCharger : Interactable
{
    public int EnergyInventory
    {
        get { 
            return _energyInventory;
        }
        set {
            if (value == 0) {
                promptUIText = "No fuel for recharge";
            }
            else {
                promptUIText = "Recharge (you have " + value + "% fuel)";
            }
            _energyInventory = value;
        } 
    }
    public Slider EnergySlider;

    private int _energyInventory;

    private void Awake() {
        EnergyInventory = 0;
    }

    public override void Interact() {
        base.Interact();
        if(EnergyInventory != 0){
            DOTween.To(() => EnergySlider.value, x => EnergySlider.value = x, Mathf.Min(EnergySlider.value + EnergyInventory, 100), 1.5f)
                .SetEase(Ease.OutQuint);
            EnergyInventory = 0;
        }
    }
}
