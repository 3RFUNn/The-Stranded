using UnityEngine;
using DG.Tweening; // Include DOTween namespace

public class HealthPickup : MonoBehaviour
{
    PlayerHealth playerHealth; // Reference to the player's health script
    public int healthBonus = 10; // Amount of health restored

    // Animation settings
    public float bobbingHeight = 0.5f; // Vertical movement height for the bobbing animation
    public float bobbingDuration = 1.5f; // Duration for one bobbing cycle
    public float rotationSpeed = 2f; // Speed of rotation (higher value = faster rotation)

    // Audio settings
    public AudioSource audioSource; // The audio source component
    public AudioClip audioClip; // The sound to play on pickup

    void Awake()
    {
        // Find the player and get the PlayerHealth component
        playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
    }

    void Start()
    {
    
    }

    void OnTriggerEnter(Collider other)
    {
        // Check if the colliding object is the player
        if (other.CompareTag("Player"))
        {
            // Ensure the player's health isn't already full
            if (playerHealth.currentHealth < playerHealth.maxHealth)
            {
                // Play the pickup sound effect
                if (audioSource != null && audioClip != null)
                {
                    GamePhaseManager.instance.GenericAudioSource.PlayOneShot(audioClip);
                }

                // Hide the health pickup object and clean up
                gameObject.SetActive(false);
                Debug.Log("Health boost picked up!");
                playerHealth.IncreaseHealth(healthBonus);
                Debug.Log("Current health: " + playerHealth.currentHealth);

                // Destroy the object after a short delay (to ensure audio finishes playing)
                Destroy(gameObject, 0.5f);
            }
        }
    }
}
