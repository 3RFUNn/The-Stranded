using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class CrushingWall : MonoBehaviour
{
    [SerializeField] private Transform targetPosition; // The position the wall will move toward when crushing
    [SerializeField] private float crushDuration = 3f; // Duration of the wall movement
    [SerializeField] private GameObject player; // The player to handle losing the game

    private Vector3 initialPosition; // The wall's starting position
    private bool isCrushing = false; // Whether the wall is actively crushing inward

    private void Start()
    {
        // Store the initial position of the wall
        initialPosition = transform.position;
    }

    private void Update()
    {
        // If the wall is crushing and DOTween hasn't been started, start the movement
        if (isCrushing && !DOTween.IsTweening(transform))
        {
            CrushWall();
        }
    }

    private void CrushWall()
    {
        // Move the wall to the target position using DOTween
        transform.DOMove(targetPosition.position, crushDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            Debug.Log("Crushing Complete!");
        });
    }

    private void OnCollisionEnter(Collision collision)
    {
        // Check if the wall touches an object with the tag "Pillar"
        if (collision.gameObject.CompareTag("Pillar"))
        {
            GameOver();
        }
    }

    private void GameOver()
    {
        Debug.Log("Game Over! The wall crushed the pillar.");
        // Handle game over logic here, such as stopping the game or showing a game over screen
        Destroy(player); // Example: Destroy the player object
        isCrushing = false; // Stop crushing after game over
    }

    public void StartCrushing()
    {
        // Method to activate the crushing wall
        isCrushing = true;
        CrushWall();
    }

    public void ResetWall()
    {
        // Method to reset the wall to its original position
        isCrushing = false;
        DOTween.Kill(transform); // Stop any ongoing DOTween animations
        transform.DOMove(initialPosition, crushDuration).SetEase(Ease.InOutQuad).OnComplete(() =>
        {
            Debug.Log("Wall reset to initial position.");
        });
    }
}
