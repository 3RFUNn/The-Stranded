using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Specific Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float preferredDistance = 8f;

    protected override void HandlePursuing()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer < preferredDistance)
        {
            // Move away to maintain preferred distance
            Vector3 directionFromPlayer = transform.position - player.position;
            Vector3 targetPosition = player.position + directionFromPlayer.normalized * preferredDistance;
            
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, preferredDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
            }
        }
        else
        {
            // Move closer if too far
            base.HandlePursuing();
        }
    }

    protected override void PerformAttack()
    {
        lastAttackTime = Time.time;
        UpdateAnimation("IsAttacking", true);

        if (projectilePrefab != null)
        {
            // Spawn and shoot projectile
            GameObject projectile = Instantiate(projectilePrefab, transform.position + transform.forward + Vector3.up, Quaternion.identity);
            Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
            
            if (projectileRb != null)
            {
                Vector3 direction = (player.position - transform.position).normalized;
                projectileRb.velocity = direction * projectileSpeed;
            }

            // Play attack sound
            if (attackSounds.Length > 0)
            {
                PlayRandomSound(attackSounds);
            }
        }
    }
}
