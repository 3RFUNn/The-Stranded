using UnityEngine;
using TMPro;

public class RotatePillarImproved : MonoBehaviour
{
    [Header("Settings")]
    public float detectionRange = 3f; // Range to detect the pillar or its child mirror
    [Range(0f, 360f)] // Allows you to adjust in the Inspector with a slider
    public float rotationAngle = 15f; // Angle to rotate the pillar

    public TextMeshProUGUI interactionText; // Text UI to display "Press T to Rotate"

    private Transform currentPillar; // Stores the currently detected pillar

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Hide interaction text initially
        if (interactionText != null)
        {
            interactionText.enabled = false;
        }
    }

    void Update()
    {
        if(!AppHelper.gameEnded){
            DetectPillarOrMirror();
            HandleInteraction();
        }
    }

    void DetectPillarOrMirror()
    {
        // Cast a sphere to detect nearby objects with the "Pillar" or "Mirror" tag
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, detectionRange);
        currentPillar = null; // Reset current pillar

        foreach (Collider collider in hitColliders)
        {
            if (collider.CompareTag("Pillar"))
            {
                currentPillar = collider.transform; // Found a pillar
                break;
            }

            // Check if the detected object is a "Mirror" and its parent is a pillar
            if (collider.CompareTag("Mirror") && collider.transform.parent != null && collider.transform.parent.CompareTag("Pillar"))
            {
                currentPillar = collider.transform.parent; // Set the parent pillar as the target
                break;
            }
        }

        // Toggle interaction text based on detection
        if (interactionText != null)
        {
            interactionText.enabled = currentPillar != null;
            if (currentPillar != null)
            {
                interactionText.text = "T: Clockwise | Y: Anti-Clockwise";
            }
        }
    }

    void HandleInteraction()
    {
        if (currentPillar != null)
        {
            if (Input.GetKeyDown(KeyCode.T))
            {
                // Rotate the pillar clockwise
                RotateObject(currentPillar, rotationAngle);
            }
            else if (Input.GetKeyDown(KeyCode.Y))
            {
                // Rotate the pillar anti-clockwise
                RotateObject(currentPillar, -rotationAngle);
            }
        }
    }

    void RotateObject(Transform pillar, float angle)
    {
        // Rotate the pillar by the specified angle
        pillar.Rotate(Vector3.up, angle);
    }

    // Draw detection range in the scene view for debugging
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}
