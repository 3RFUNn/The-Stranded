using System;
using UnityEngine;

public class MagzinePickup : MonoBehaviour
{
    private bool isResetCalled = false;
    private GamePhaseManager _gamePhaseManager;
    
    public InventoryManager Manager;

    private void Awake()
    {
        Manager = InventoryManager.instance;
    }

    public void Initialize(GamePhaseManager gamePhaseManager)
    {
        _gamePhaseManager = gamePhaseManager;
    }
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") && !isResetCalled)
        {
            isResetCalled = true;
            Debug.Log("Called Reset Gun!");
           // _gamePhaseManager.gunSystem.ResetGun();
           
           Manager.AddBullets();
           
            Destroy(gameObject, 0.5f);
        }
    }
}
