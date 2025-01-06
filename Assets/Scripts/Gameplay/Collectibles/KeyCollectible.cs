using UnityEngine;

public class KeyCollectible : ItemCollectible
{
    
    
    public override void Interact()
    {
        base.Interact();
        OnKeyCollected();
    }

    private void OnKeyCollected()
    {
        // Add your custom logic here
        Debug.Log("Key collected!");
        AppHelper.HasTalkedFinalNPC = true;
        // Example: Unlock a door or trigger an event
    }
}