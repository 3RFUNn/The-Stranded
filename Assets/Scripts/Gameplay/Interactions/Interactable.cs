using UnityEngine;

namespace Gameplay.Interactions
{
    public class Interactable : MonoBehaviour
    {
        public string PromptTextInUI;
        public virtual void Interact()
        {
            // Base interaction method. This will be overridden by derived classes.
            Debug.Log("Interacting with " + gameObject.name);
        }
    }
}
