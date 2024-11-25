using UnityEngine;

namespace Gameplay.Interactions
{
    public abstract class Interactable : MonoBehaviour
    {
        public string promptUIText;
        public bool ShouldShowPrompt = true;
        public virtual void Interact()
        {
            // Base interaction method. This will be overridden by derived classes.
            Debug.Log("Interacting with " + gameObject.name);
        }
    }
}
