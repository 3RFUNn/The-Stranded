using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;
using UnityEngine.InputSystem;

public class CollectableGun : ItemCollectible
{
    public GameObject gunCollectableGO;
    public GameObject gunInHandGO;
    public GameObject lightGO;
    public GameObject bulletCountUI;
    public PlayerInput Input;

    private void Start() {
        Input.actions.FindAction("Fire").Disable();
        Input.actions.FindAction("Reload").Disable();
        gunInHandGO.SetActive(false);
        bulletCountUI.SetActive(false);
    }
    public override void Interact()
    {
        base.Interact();
        Destroy(gunCollectableGO);
        Destroy(lightGO);
        Input.actions.FindAction("Fire").Enable();
        Input.actions.FindAction("Reload").Enable();
        gunInHandGO.SetActive(true);
        bulletCountUI.SetActive(true);
    }
}
