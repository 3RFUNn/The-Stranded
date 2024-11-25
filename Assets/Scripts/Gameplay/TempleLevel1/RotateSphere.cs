using UnityEngine;
using DG.Tweening;

public class RotateSphere : MonoBehaviour
{
    [SerializeField]
    private float rotationDuration = 2f; // Duration for one complete rotation
    [SerializeField]
    private bool clockwise = true; // Rotate clockwise or counter-clockwise

    void Start()
    {
        RotateAlongYAxis();
    }

    void RotateAlongYAxis()
    {
        float rotationAngle = clockwise ? 360f : -360f;

        // Rotate the sphere continuously along the Y-axis
        transform.DORotate(new Vector3(0, 0, rotationAngle), rotationDuration, RotateMode.LocalAxisAdd)
            .SetEase(Ease.Linear) // Smooth linear rotation
            .SetLoops(-1, LoopType.Restart); // Infinite loop
    }
}
