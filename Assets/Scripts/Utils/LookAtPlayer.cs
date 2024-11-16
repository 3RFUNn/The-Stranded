using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LookAtPlayer : MonoBehaviour
{
    [SerializeField] private GameObject player;

    public bool onlyChangeYRotation = false;

    void Update()
    {
        if(onlyChangeYRotation){
            // Get the direction to the player
            Vector3 directionToPlayer = player.transform.position - transform.position;
            
            // Zero out the Y-axis rotation for the object's own position
            directionToPlayer.y = 0;

            // Ensure the direction vector is normalized (to avoid scaling issues)
            directionToPlayer.Normalize();

            // Compute the new rotation based on the direction
            Quaternion targetRotation = Quaternion.LookRotation(directionToPlayer);

            // Apply the rotation only to the Y axis
            transform.rotation = Quaternion.Euler(0, targetRotation.eulerAngles.y, 0);
        }else{
            transform.LookAt(player.transform);
        }
    }
}
