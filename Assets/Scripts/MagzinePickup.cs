using System;
using UnityEngine;

public class MagzinePickup : MonoBehaviour
{
    private bool isResetCalled = false;
    private GamePhaseManager _gamePhaseManager;
    
    private InventoryManager Manager;


    public void Initialize(GamePhaseManager gamePhaseManager)
    {
        _gamePhaseManager = gamePhaseManager;
    }
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player") && !isResetCalled)
        {
            if(GamePhaseManager.instance != null)
            {
                _gamePhaseManager = GamePhaseManager.instance;
            }
            Manager = _gamePhaseManager.inventoryManager;
            _gamePhaseManager.gunSystem.PlayReloadSound();
            isResetCalled = true;
            Debug.Log("Called Reset Gun!");
           // _gamePhaseManager.gunSystem.ResetGun();
           
           Manager.AddBullets();
           
            Destroy(gameObject, 0.5f);
        }
    }
}
