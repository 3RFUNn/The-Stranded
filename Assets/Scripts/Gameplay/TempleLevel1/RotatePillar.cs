using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class RotatePillar: MonoBehaviour
{
    public static RotatePillar instance;    

    [Header("Settings")]
    public float detectionRange = 3f; // Range to detect the pillar or its child mirror
    [Range(0f, 360f)] // Allows you to adjust in the Inspector with a slider
    public float rotationAngle = 15f; // Angle to rotate the pillar incrementally
    public float continuousRotationSpeed = 45f; // Speed for continuous rotation in degrees per second

    public GameObject interactionText; // Text UI to display "Press T to Rotate"
    public Transform currentPillar; // Stores the currently detected pillar

    private float _inputAxis;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        instance = this;
    }

    void Update()
    {
        //if (!AppHelper.gameEnded)
        //{
        //    DetectPillarOrMirror();
        //    HandleInteraction();
        //}else{
        //    interactionText.SetActive(false);
        //}
        if(currentPillar != null){
            RotateObject(currentPillar, _inputAxis * continuousRotationSpeed * Time.deltaTime);
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
            interactionText.SetActive(currentPillar != null);
        }
    }

    void HandleInteraction()
    {
        if (currentPillar != null)
        {
            if (Input.GetKey(KeyCode.T))
            {
                // Rotate the pillar clockwise continuously
                RotateObject(currentPillar, continuousRotationSpeed * Time.deltaTime);
            }
            else if (Input.GetKey(KeyCode.Y))
            {
                // Rotate the pillar anti-clockwise continuously
                RotateObject(currentPillar, -continuousRotationSpeed * Time.deltaTime);
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

    public void OnRotate(InputAction.CallbackContext ctx) {
        _inputAxis = ctx.ReadValue<float>();
    }
}
