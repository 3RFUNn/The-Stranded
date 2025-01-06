using UnityEngine;

public class KeyCollectible : ItemCollectible
{
    public GameObject player; // Reference to the player game object
    public GameObject checkpoint; // Reference to the checkpoint game object
    public AudioClip stopmusic; // Reference to the audio clip to stop
    private AudioSource audioSource; // Reference to the AudioSource component

    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
    }

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
        TransferPlayerToCheckpoint();
        StopMusic();
    }

    private void TransferPlayerToCheckpoint()
    {
        if (player != null && checkpoint != null)
        {
            player.transform.position = checkpoint.transform.position;
            Debug.Log("Player position transferred to checkpoint.");
        }
        else
        {
            Debug.LogWarning("Player or checkpoint reference is missing.");
        }
    }

    private void StopMusic()
    {
        if (audioSource != null && stopmusic != null)
        {
            audioSource.Stop();
            Debug.Log("Music stopped.");
        }
        else
        {
            Debug.LogWarning("AudioSource or stopmusic reference is missing.");
        }
    }
}