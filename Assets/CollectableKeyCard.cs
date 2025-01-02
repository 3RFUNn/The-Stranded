using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;


public class CollectableKeyCard : ItemCollectible
{
    private void Awake()
    {
        PromptUIText = "Collect Access Card";
    }

    public override void Interact()
    {
        base.Interact();
        AppHelper.hasKey = true;
        
    }

}
