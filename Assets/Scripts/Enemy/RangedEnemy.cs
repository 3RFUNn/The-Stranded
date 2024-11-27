using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Combat Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private float distanceThreshold = 1.5f;  // How close to preferred distance is acceptable

    [Header("Positioning Settings")]
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float strafeInterval = 2f;
    private float nextStrafeTime;
    private bool isStrafeLeft;

    protected override void Start()
    {
        base.Start();
        
        if (projectilePrefab == null)
        {
            Debug.LogError($"Projectile prefab missing on {gameObject.name}!");
            enabled = false;
            return;
        }

        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
            Debug.LogWarning($"No projectile spawn point set on {gameObject.name}, using transform as default.");
        }

        // Set appropriate speeds
        walkSpeed = strafeSpeed;
        runSpeed = maxSpeed;
    }

    protected override void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Always face the player
        FaceTarget(player.position);

        // Handle movement based on distance to player
        if (Mathf.Abs(distanceToPlayer - preferredDistance) <= distanceThreshold)
        {
            // At ideal range - strafe and potentially attack
            HandleStrafe();
            
            if (CheckLineOfSight() && Time.time >= lastAttackTime + attackInterval)
            {
                ChangeState(EnemyState.Attacking);
                return;
            }
        }
        else if (distanceToPlayer < preferredDistance - distanceThreshold)
        {
            // Too close - back away
            Vector3 retreatDirection = transform.position - player.position;
            Vector3 targetPosition = player.position + retreatDirection.normalized * preferredDistance;
            
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, preferredDistance, NavMesh.AllAreas))
            {
                MoveToPoint(hit.position, runSpeed);
                UpdateAnimationState(false, false, true, false);
            }
        }
        else
        {
            // Too far - move closer
            MoveToPoint(player.position, runSpeed);
            UpdateAnimationState(false, false, true, false);
        }
    }

    private void HandleStrafe()
    {
        if (Time.time >= nextStrafeTime)
        {
            isStrafeLeft = !isStrafeLeft;
            nextStrafeTime = Time.time + strafeInterval;
        }

        Vector3 strafeDirection = isStrafeLeft ? -transform.right : transform.right;
        Vector3 targetPosition = transform.position + strafeDirection * strafeSpeed;

        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, strafeSpeed, NavMesh.AllAreas))
        {
            MoveToPoint(hit.position, strafeSpeed);
            UpdateAnimationState(false, true, false, false);
        }
    }

    protected override void HandleAttacking()
    {
        if (player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLineOfSight = CheckLineOfSight();

        // Check if we should keep attacking
        if (!hasLineOfSight || Mathf.Abs(distanceToPlayer - preferredDistance) > distanceThreshold)
        {
            ChangeState(EnemyState.Pursuing);
            return;
        }

        // Stop movement during attack
        StopMovement();
        
        // Keep facing the player
        FaceTarget(player.position);

        // Update animation
        UpdateAnimationState(false, false, false, true);

        // Perform attack if cooldown is over
        if (Time.time >= lastAttackTime + attackInterval)
        {
            FireProjectile();
            lastAttackTime = Time.time;
        }
    }

    private void FireProjectile()
    {
        if (projectilePrefab == null || player == null) return;

        // Play attack sound
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }

        // Calculate spawn position
        Vector3 spawnPosition = projectileSpawnPoint.position;

        // Calculate target position with basic prediction
        Vector3 targetPosition = player.position;
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            float distanceToTarget = Vector3.Distance(spawnPosition, player.position);
            float travelTime = distanceToTarget / projectileSpeed;
            targetPosition += playerRb.velocity * travelTime;
        }

        // Calculate firing direction
        Vector3 directionToTarget = (targetPosition - spawnPosition).normalized;
        Quaternion projectileRotation = Quaternion.LookRotation(directionToTarget);

        // Spawn and launch projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, projectileRotation);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        
        if (projectileRb != null)
        {
            // Add slight upward arc to the projectile
            Vector3 firingDirection = (directionToTarget + Vector3.up * 0.1f).normalized;
            projectileRb.velocity = firingDirection * projectileSpeed;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw preferred range
        Gizmos.color = new Color(0f, 1f, 1f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, preferredDistance);
        
        // Draw preferred range threshold
        Gizmos.color = new Color(0f, 0.5f, 0.5f, 0.2f);
        Gizmos.DrawWireSphere(transform.position, preferredDistance - distanceThreshold);
        Gizmos.DrawWireSphere(transform.position, preferredDistance + distanceThreshold);
    }
}