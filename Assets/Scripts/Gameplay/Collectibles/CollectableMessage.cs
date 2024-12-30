using Gameplay.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CollectableMessage : Interactable
{
    public MessageSO Message;
    public PlayerTablet PlayerTablet;
    void Awake()
    {
        PromptUIText = "Collect";
        //PlayerTablet = FindFirstObjectByType<PlayerTablet>();
    }

    public override void Interact() {
        base.Interact();
        PlayerTablet.AddMessage(Message);
        Destroy(this.gameObject);
    }
}
