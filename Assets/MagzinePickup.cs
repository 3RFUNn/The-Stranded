using UnityEngine;

public class MagzinePickup : MonoBehaviour
{
    private GamePhaseManager _gamePhaseManager;

    public void Initialize(GamePhaseManager gamePhaseManager)
    {
        _gamePhaseManager = gamePhaseManager;
    }
    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            _gamePhaseManager.gunSystem.ResetGun();
            Destroy(gameObject, 0.5f);
        }
    }
}
