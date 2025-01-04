using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System;

public class ItemCollectible : Interactable
{
    private GameObject floaterMessagePrefab; // Prefab for the floater message
    private Transform messageSpawnPoint; // Point to spawn the floater message
    public string itemName; // Set this in the Inspector, e.g., "Wood" or "Stone"
    [SerializeField] private GameObject player;

    public string PostCollectText = "Collected!";

    public override void Interact()
    {
        InventoryManager inventory = player.GetComponent<InventoryManager>();
            
        if (inventory != null)
        {
            inventory.CollectItem(itemName);
            PlaySound();
            Debug.Log(itemName + " collected");
            ShowFloaterMessage();
            Destroy(gameObject); // Destroy the collectible item after picking it up
        }
    }

    

    public void ShowFloaterMessage()
    {
        Debug.Log("Floater Called!");
        floaterMessagePrefab = GamePhaseManager.instance.FloaterText;
        messageSpawnPoint = GamePhaseManager.instance.FloaterMessageSpawnPoint;

        if (floaterMessagePrefab != null && messageSpawnPoint != null)
        {
            // Check if there are any children under messageSpawnPoint and destroy them
            foreach (Transform child in messageSpawnPoint)
            {
                Destroy(child.gameObject);
            }

            // Instantiate a new floater as a child of messageSpawnPoint
            GameObject floater = Instantiate(floaterMessagePrefab, messageSpawnPoint.position, Quaternion.identity, messageSpawnPoint);
            Debug.Log("Floater Instantiated!");
            TextMeshProUGUI messageText = floater.GetComponentInChildren<TextMeshProUGUI>();
            if (messageText != null)
            {
                messageText.text = PostCollectText;
            }

            RectTransform floaterRect = floater.GetComponent<RectTransform>();
            if (floaterRect != null)
            {
                floaterRect.DOAnchorPosY(floaterRect.anchoredPosition.y + 50f, 2f).SetEase(Ease.OutQuad);
                CanvasGroup canvasGroup = floater.GetComponent<CanvasGroup>();
                if (canvasGroup != null)
                {
                    canvasGroup.DOFade(0, 2f).OnComplete(() => Destroy(floater));
                }
                else
                {
                    Destroy(floater, 2f);
                }
            }
        }
        else
        {
            Debug.LogError("Floater message prefab or spawn point not assigned.");
        }
    }

}
