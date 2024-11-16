using Gameplay.Interactions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollectableMessage : Interactable
{
    public MessageSO message;
    public PlayerTablet playerTablet;
    // Start is called before the first frame update
    void Start()
    {
        promptUIText = "collect";
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void Interact() {
        base.Interact();
        playerTablet.AddMessage(message);
        Destroy(this.gameObject);
    }
}
