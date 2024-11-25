using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Specific Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private float repositionThreshold = 2f;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float strafeSpeed = 4f;
    private Vector3 strafeTarget;
    private bool isStrafeLeft = false;
    private float nextStrafeChangeTime;
    private float strafeChangeInterval = 2f;

    protected override void Start()
    {
        base.Start();
        if (projectilePrefab == null)
        {
            Debug.LogError($"Projectile prefab missing on {gameObject.name}!");
            enabled = false;
        }
    }

    protected override void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Always face the player
        FaceTarget(player.position);

        if (Mathf.Abs(distanceToPlayer - preferredDistance) <= repositionThreshold)
        {
            // At ideal range, strafe
            HandleStrafing();
        }
        else if (distanceToPlayer < preferredDistance - repositionThreshold)
        {
            // Too close, back away
            Vector3 backawayDirection = transform.position - player.position;
            Vector3 targetPosition = player.position + backawayDirection.normalized * preferredDistance;
            
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, preferredDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                UpdateAnimation("IsRunning", true);
            }
        }
        else
        {
            // Too far, move closer
            agent.SetDestination(player.position);
            UpdateAnimation("IsRunning", true);
        }

        // Check if we can attack from current position
        if (CheckLineOfSight() && distanceToPlayer <= detectionRange &&
            Mathf.Abs(distanceToPlayer - preferredDistance) <= repositionThreshold)
        {
            if (Time.time >= lastAttackTime + attackInterval)
            {
                StartCoroutine(SmoothStateTransition(EnemyState.Attacking));
            }
        }
    }

    private void HandleStrafing()
    {
        if (Time.time >= nextStrafeChangeTime)
        {
            isStrafeLeft = !isStrafeLeft;
            nextStrafeChangeTime = Time.time + strafeChangeInterval;
        }

        Vector3 strafeDirection = isStrafeLeft ? -transform.right : transform.right;
        Vector3 targetPosition = transform.position + strafeDirection * strafeSpeed;

        // Ensure strafe position is valid
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, strafeSpeed, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            UpdateAnimation("IsWalking", true);
        }
    }

    protected override void HandleAttacking()
    {
        // Stop moving when attacking
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Keep facing the player
        FaceTarget(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool hasLineOfSight = CheckLineOfSight();

        // Only attack if conditions are right
        if (Time.time >= lastAttackTime + attackInterval && hasLineOfSight &&
            Mathf.Abs(distanceToPlayer - preferredDistance) <= repositionThreshold)
        {
            UpdateAnimation("IsAttacking", true);
            PerformAttack();
        }
        else if (!hasLineOfSight || Mathf.Abs(distanceToPlayer - preferredDistance) > repositionThreshold)
        {
            StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
        }
    }

    protected override void PerformAttack()
    {
        if (projectilePrefab == null || player == null) return;

        lastAttackTime = Time.time;
        
        // Play attack sound
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }

        // Calculate spawn position
        Vector3 spawnPosition = projectileSpawnPoint != null 
            ? projectileSpawnPoint.position 
            : transform.position + transform.forward + Vector3.up;

        // Spawn projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        
        if (projectileRb != null)
        {
            // Calculate direction with prediction
            Vector3 targetPosition = player.position + (player.GetComponent<Rigidbody>()?.velocity ?? Vector3.zero) * 
                                   (Vector3.Distance(spawnPosition, player.position) / projectileSpeed);
            
            Vector3 directionToTarget = (targetPosition - spawnPosition).normalized;
            Vector3 arcedDirection = (directionToTarget + Vector3.up * 0.1f).normalized;
            
            projectileRb.velocity = arcedDirection * projectileSpeed;
        }

        StartCoroutine(ResetAttackAnimation());
    }

    protected override void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
        base.UpdateState(distanceToPlayer, canSeePlayer);

        // Additional check for losing line of sight
        if (currentState == EnemyState.Attacking && !canSeePlayer)
        {
            StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
        }
    }
}