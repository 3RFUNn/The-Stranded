using DG.Tweening;
using Gameplay.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorSwitch : Interactable
{
    public Transform Door, Dest;
    public float Duration = 1f;
    public override void Interact() {
        base.Interact();
        Door.DOMove(Dest.position, Duration);
    }

}
