using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;

public class CollectableTablet : ItemCollectible
{
    public GameObject tabletGO;
    public override void Interact()
    {
        base.Interact();
        tabletGO.SetActive(true);
    }
}
