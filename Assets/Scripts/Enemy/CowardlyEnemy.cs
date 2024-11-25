using UnityEngine;
using UnityEngine.AI;

public class CowardlyEnemy : BaseEnemy
{
    [Header("Cowardly Specific Settings")]
    [SerializeField] private float panicThreshold = 8f;
    [SerializeField] private float fleeSpeed = 10f;
    [SerializeField] private float fleeRotationSpeed = 10f;
    [SerializeField] private float calmDownTime = 5f;
    private float timeStartedFleeing;
    private bool isPanicked = false;

    protected override void Start()
    {
        base.Start();
        maxSpeed = fleeSpeed;
    }

    protected override void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
        // Handle panic state first
        if (canSeePlayer && distanceToPlayer <= panicThreshold)
        {
            if (!isPanicked)
            {
                timeStartedFleeing = Time.time;
                isPanicked = true;
            }
            StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
            return;
        }

        // If we're fleeing, check if we should calm down
        if (currentState == EnemyState.Fleeing)
        {
            if (distanceToPlayer >= safeDistance || (!canSeePlayer && Time.time - timeStartedFleeing >= calmDownTime))
            {
                isPanicked = false;
                StartCoroutine(SmoothStateTransition(EnemyState.Patrolling));
                return;
            }
        }

        // Only allow transitions to Patrolling or Fleeing
        if (currentState != EnemyState.Fleeing && currentState != EnemyState.Patrolling)
        {
            StartCoroutine(SmoothStateTransition(EnemyState.Patrolling));
        }
    }

    protected override void HandleFleeing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        // Calculate flee direction (away from player)
        Vector3 fleeDirection = transform.position - player.position;
        fleeDirection.y = 0;
        Vector3 targetPosition = transform.position + fleeDirection.normalized * safeDistance;

        // Find valid position on NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, safeDistance, NavMesh.AllAreas))
        {
            agent.isStopped = false;
            agent.SetDestination(hit.position);
            
            // Always face away from the player while fleeing
            Vector3 lookDirection = transform.position + fleeDirection;
            Quaternion targetRotation = Quaternion.LookRotation(fleeDirection.normalized);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * fleeRotationSpeed);
            
            UpdateAnimation("IsRunning", true);
        }
    }

    protected override void HandlePursuing()
    {
        // Override to prevent any pursuit behavior
        StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
    }

    protected override void HandleAttacking()
    {
        // Override to prevent any attack behavior
        StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
    }

    protected override void PerformAttack()
    {
        // No attack behavior for cowardly enemy
    }

    protected override void UpdateSpeedAndAnimation()
    {
        float targetSpeed = currentState == EnemyState.Fleeing ? fleeSpeed : walkSpeed;
        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedAcceleration);
        agent.speed = currentSpeed;

        // Update animations
        if (animator != null)
        {
            animator.SetBool("IsIdle", currentSpeed < 0.1f);
            animator.SetBool("IsWalking", currentSpeed >= 0.1f && currentSpeed <= walkSpeed * 1.5f);
            animator.SetBool("IsRunning", currentSpeed > walkSpeed * 1.5f);
        }
    }
}