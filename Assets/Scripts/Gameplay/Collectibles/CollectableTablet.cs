using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectableTablet : ItemCollectible
{
    public GameObject tabletGO;
    public PlayerInput input;

    private void Start() {
        input.actions.FindAction("Tablet").Disable();
        tabletGO.SetActive(false);
    }
    public override void Interact()
    {
        base.Interact();
        tabletGO.SetActive(true);
        tabletGO.GetComponent<PlayerTablet>().SetMessage(0);
        input.actions.FindAction("Tablet").Enable();
    }
}
