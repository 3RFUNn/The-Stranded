using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;
    [SerializeField] private bool isEnemyHealthBar = false;

    public bool onlyChangeYRotation = false;

    private void Start()
    {
        if (player == null)
        {
            if (GamePhaseManager.instance != null)
            {
                player = GamePhaseManager.instance.playerCamera;
            }
            else
            {
                GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
                if (playerObject != null)
                {
                    player = playerObject;
                }
                else
                {
                    Debug.LogWarning("No GameObject with the tag 'Player' found in the hierarchy.");
                }
            }
        }
    }

    void Update()
    {
        if (player == null) return;

        if (onlyChangeYRotation)
        {
            // Get the direction to the player
            Vector3 directionToPlayer = player.transform.position - transform.position;

            // Invert direction if it's an enemy health bar
            if (isEnemyHealthBar)
            {
                directionToPlayer = -directionToPlayer;
            }

            // Zero out the Y-axis rotation for the object's own position
            directionToPlayer.y = 0;

            // Ensure the direction vector is normalized (to avoid scaling issues)
            directionToPlayer.Normalize();

            // Compute the new rotation based on the direction
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // Apply the rotation only to the Y axis
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }
        else
        {
            // Regular LookAt or inverted LookAt
            if (isEnemyHealthBar)
            {
                // Look away from the player
                Vector3 directionAwayFromPlayer = transform.position - player.transform.position;
                Quaternion lookAwayRotation = Quaternion.LookRotation(directionAwayFromPlayer);
                transform.rotation = lookAwayRotation;
            }
            else
            {
                // Look at the player
                transform.LookAt(player.transform);
            }
        }
    }
}
