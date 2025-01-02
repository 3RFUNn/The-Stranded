using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;

public class ItemCollectible : Interactable
{
    public string itemName; // Set this in the Inspector, e.g., "Wood" or "Stone"
    [SerializeField] private GameObject player;
    public AudioSource audioSource;

    public AudioClip audioClip;
    

    public override void Interact()
    {
        InventoryManager inventory = player.GetComponent<InventoryManager>();
            
        if (inventory != null)
        {
            inventory.CollectItem(itemName);
            if (audioSource != null && audioClip != null)
            {
                audioSource.PlayOneShot(audioClip);
            }
            Debug.Log(itemName + " collected");
            Destroy(gameObject); // Destroy the collectible item after picking it up
        }



        
    }
}
