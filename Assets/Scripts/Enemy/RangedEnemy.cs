using UnityEngine;
using UnityEngine.AI;

public class RangedEnemy : BaseEnemy
{
    [Header("Ranged Specific Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private float normalSpeed = 3f;
    [SerializeField] private Transform projectileSpawnPoint; // Optional: for better projectile spawning

    protected override void Start()
    {
        base.Start();
        agent.speed = normalSpeed;
        
        // Verify required components
        if (projectilePrefab == null)
        {
            Debug.LogError($"Projectile prefab missing on {gameObject.name}!");
        }
    }

    protected override void HandlePursuing()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        if (distanceToPlayer < preferredDistance - 1f)
        {
            // Back away if too close
            Vector3 directionFromPlayer = transform.position - player.position;
            Vector3 targetPosition = player.position + directionFromPlayer.normalized * preferredDistance;
            
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, preferredDistance, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                UpdateAnimation("IsWalking", true);
            }
        }
        else if (distanceToPlayer > preferredDistance + 1f)
        {
            // Move closer if too far
            agent.SetDestination(player.position);
            UpdateAnimation("IsRunning", true);
        }
        else
        {
            // At ideal range, stop and attack
            StopAndRotate(player.position);
            if (Time.time >= lastAttackTime + attackInterval)
            {
                StartCoroutine(SmoothStateTransition(EnemyState.Attacking));
            }
        }
    }

    protected override void HandleAttacking()
    {
        // Stop moving when attacking
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Keep facing the player
        FaceTarget(player.position);

        // Check if we can attack
        if (Time.time >= lastAttackTime + attackInterval)
        {
            animator.SetBool("IsAttacking", true);
            PerformAttack();
        }

        // Check if we should return to pursuing
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (Mathf.Abs(distanceToPlayer - preferredDistance) > 2f)
        {
            StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
        }
    }

    protected override void PerformAttack()
    {
        if (projectilePrefab == null) return;

        lastAttackTime = Time.time;
        
        // Play attack animation and sound
        UpdateAnimation("IsAttacking", true);
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }

        // Calculate spawn position
        Vector3 spawnPosition;
        if (projectileSpawnPoint != null)
        {
            spawnPosition = projectileSpawnPoint.position;
        }
        else
        {
            spawnPosition = transform.position + transform.forward + Vector3.up;
        }

        // Spawn and configure projectile
        GameObject projectile = Instantiate(projectilePrefab, spawnPosition, Quaternion.identity);
        Rigidbody projectileRb = projectile.GetComponent<Rigidbody>();
        
        if (projectileRb != null)
        {
            // Calculate direction to player with slight upward arc
            Vector3 directionToPlayer = (player.position - spawnPosition).normalized;
            Vector3 arcedDirection = (directionToPlayer + Vector3.up * 0.1f).normalized;
            
            // Apply velocity to projectile
            projectileRb.velocity = arcedDirection * projectileSpeed;
        }

        // Start coroutine to reset attack animation
        StartCoroutine(ResetAttackAnimation());
    }

    protected override void UpdateState(float distanceToPlayer)
    {
        // Only pursue or attack if within detection range but beyond flee threshold
        if (distanceToPlayer <= detectionRange)
        {
            if (currentState == EnemyState.Patrolling)
            {
                StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
            }
        }
        else
        {
            if (currentState != EnemyState.Patrolling)
            {
                StartCoroutine(SmoothStateTransition(EnemyState.Patrolling));
            }
        }
    }
}