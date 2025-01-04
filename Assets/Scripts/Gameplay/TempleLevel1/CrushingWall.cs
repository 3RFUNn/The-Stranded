using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;

public class CrushingWall : MonoBehaviour
{
    [SerializeField] private Transform targetPosition; // The position the wall will move toward when crushing
    [SerializeField] private float crushDuration = 180f; // Duration of the wall movement (3 minutes)
    [SerializeField] private GameObject player; // The player to handle losing the game
    [SerializeField] private GameObject EndScreenUI;
    [SerializeField] private TextMeshProUGUI text;

    private Vector3 initialPosition; // The wall's starting position
    private bool isCrushing = false; // Whether the wall is actively crushing inward
    private bool hasCrushed = false; // Prevents multiple crushing animations

    private Tween crushingTween; // Reference to the active DOTween animation

    private void Start()
    {
        // Store the initial position of the wall
        initialPosition = transform.position;

        // Debug to ensure targetPosition is assigned
        if (targetPosition == null)
        {
            Debug.LogError("Target position is not assigned. Please assign it in the Inspector.");
        }

        StartCrushing(); // Start the crushing process
    }

    private void Update()
    {
        // Stop the animation if AppHelper.stopCrushing is true
        if (AppHelper.stopCrushing && crushingTween != null && crushingTween.IsActive() && AppHelper.hintDisplayed)
        {
            StopCrushing();
        }
    }

    private void CrushWall()
    {
        if (targetPosition == null)
        {
            Debug.LogError("Target position is not assigned. Cannot move the wall.");
            return;
        }

        // Start DOTween animation only once
        crushingTween = transform.DOMove(targetPosition.position, crushDuration)
            .SetEase(Ease.InOutQuad)
            .OnStart(() =>
            {
                Debug.Log("Crushing started.");
            })
            .OnComplete(() =>
            {
                if (!hasCrushed && !AppHelper.stopCrushing)
                {
                    hasCrushed = true;
                    Debug.Log("Crushing Complete!");
                    GameOver();
                }
            });
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the wall touches an object with the tag "Pillar"
        if (collision.gameObject.CompareTag("Pillar"))
        {
            Debug.Log("Wall collided with a pillar.");
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! The wall crushed the pillar.");
        text.color = Color.red;
        text.text = "DEFEATED!";
        EndScreenUI.SetActive(true);
        AppHelper.gameEnded = true;
        isCrushing = false; // Stop crushing after game over
    }

    public void StartCrushing()
    {
        if (isCrushing) return; // Prevent multiple calls to CrushWall
        isCrushing = true;
        CrushWall();
    }

    public void StopCrushing()
    {
        if (crushingTween != null && crushingTween.IsActive())
        {
            crushingTween.Kill(); // Stop the current animation
            Debug.Log("Crushing stopped.");
        }
        isCrushing = false;
    }

    public void ResetWall()
    {
        if (crushingTween != null && crushingTween.IsActive())
        {
            crushingTween.Kill(); // Stop the current animation
        }

        // Reset the wall's position using DOTween
        transform.DOMove(initialPosition, crushDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            Debug.Log("Wall reset to initial position.");
            isCrushing = false;
            hasCrushed = false;
        });
    }
}
