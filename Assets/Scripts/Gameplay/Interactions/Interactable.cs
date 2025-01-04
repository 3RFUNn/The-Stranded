using UnityEngine;

namespace Gameplay.Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public string PromptUIText;

        private AudioSource audioSource;

        public AudioClip audioClip;
        public bool CanInteract = true;
        public virtual void Interact()
        {
            // Base interaction method. This will be overridden by derived classes.
            PlaySound();
            Debug.Log("Interacting with " + gameObject.name);
        }

        public void PlaySound()
        {
            audioSource = GamePhaseManager.instance.GenericAudioSource;
            if (audioSource != null && audioClip != null)
            {
                audioSource.PlayOneShot(audioClip);
            }
        }
    }
}
