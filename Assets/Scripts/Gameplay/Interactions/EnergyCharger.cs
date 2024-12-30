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
            int remain = (EnergyInventory + (int)EnergySlider.value) % 100;
            DOTween.To(() => EnergySlider.value, x => EnergySlider.value = x, Mathf.Min(EnergySlider.value + EnergyInventory, 100), 1.5f)
                .SetEase(Ease.OutQuint)
                .OnComplete(()=>{
                    GamePhaseManager.instance.ToEnding();
                });
            EnergyInventory = remain;
        }
    }
}
