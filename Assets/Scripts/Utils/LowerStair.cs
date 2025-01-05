using DG.Tweening;
using Gameplay.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LowerStair : Interactable
{

    public Transform Stair;
    public float Duration;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    public override void Interact() {
        base.Interact();
        Stair.DOLocalRotate(new Vector3(30f, -180f, 0f), Duration);
        //Stair.DORotate(new Vector3(30f, -180f, 0f), Duration);
        CanInteract = false;

    }
}
