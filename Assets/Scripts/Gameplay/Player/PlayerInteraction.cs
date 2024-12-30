using System.Collections;
using System.Collections.Generic;
using Gameplay.Interactions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerInteraction : MonoBehaviour
{
    public float interactDistance = 3f;  // Max distance to interact
    [SerializeField]
    private Interactable interactable; // Reference to interactable object
    public PlayerInput PlayerInput;
    public TextMeshProUGUI promptText;
    public RectTransform promptUI;
    public Transform CameraTransform;

    
    void Update()
    {
        CheckForInteractable();
    }

    void CheckForInteractable()
    {
        //print(Camera.main.transform.position);
        // Cast a ray from the player's position forward
        Ray ray = new Ray(CameraTransform.position, CameraTransform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            // Check if the object hit has an Interactable component
            Interactable hitInteractable = hit.collider.GetComponent<Interactable>();

            if (hitInteractable != null && hitInteractable.CanInteract)
            {
                interactable = hitInteractable;
                promptText.text = interactable.PromptUIText;
                //update the layout immediately to fit new text's length
                LayoutRebuilder.ForceRebuildLayoutImmediate(promptUI);
                promptUI.gameObject.SetActive(true);
            }
            else
            {
                interactable = null;
                promptUI.gameObject.SetActive(false);
                promptText.text = "";
                LayoutRebuilder.ForceRebuildLayoutImmediate(promptUI);
            }
        }
        else
        {
            interactable = null;
            promptUI.gameObject.SetActive(false);
            promptText.text = "";
            LayoutRebuilder.ForceRebuildLayoutImmediate(promptUI);
        }
    }

    public void PlayerInteract(InputAction.CallbackContext callback){
        if(callback.started && interactable != null)
        {
            interactable.Interact();
        }
    }

    public void OnUIExit(InputAction.CallbackContext ctx){
        //if(ctx.started){
        //    promptText.enabled = true;
        //    PlayerInput.SwitchCurrentActionMap("Player");
        //    Cursor.visible = false;
        //    Cursor.lockState = CursorLockMode.Locked;
        //}
    }
}
