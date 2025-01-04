using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CowardlyEnemy : BaseEnemy
{
    [Header("Cowardly Specific Settings")]
    [SerializeField] private float panicDistance = 8f;
    [SerializeField] private float safeDistance = 30f;
    [SerializeField] private float fleeSpeed = 10f;
    [SerializeField] private float calmDownTime = 5f;
    
    private bool isPanicked = false;
    private float timeStartedFleeing;
    private Vector3 lastSafePosition;
    private const float MIN_FLEE_DISTANCE = 5f;

    protected override void Start()
    {
        base.Start();
        maxSpeed = fleeSpeed;
        runSpeed = fleeSpeed;
        lastSafePosition = transform.position;
    }

    protected override void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
        // Immediately enter fleeing if player is too close
        if (canSeePlayer && distanceToPlayer <= panicDistance)
        {
            if (!isPanicked)
            {
                timeStartedFleeing = Time.time;
                isPanicked = true;
                ChangeState(EnemyState.Fleeing);
            }
            return;
        }

        // Check if we should exit fleeing state
        if (currentState == EnemyState.Fleeing)
        {
            bool isSafe = distanceToPlayer >= safeDistance;
            bool isCalmedDown = !canSeePlayer && Time.time - timeStartedFleeing >= calmDownTime;

            if (isSafe || isCalmedDown)
            {
                isPanicked = false;
                lastSafePosition = transform.position;
                ChangeState(EnemyState.Patrolling);
            }
        }
        // Always default to patrolling if not fleeing
        else if (currentState != EnemyState.Patrolling)
        {
            ChangeState(EnemyState.Patrolling);
        }
    }

    protected override void HandleFleeing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        // Get the direction away from player
        Vector3 fleeDirection = (transform.position - player.position).normalized;
    
        // Calculate several potential flee positions at different angles
        Vector3 bestFleePosition = FindBestFleePosition(fleeDirection);
    
        // Always face away from player while fleeing
        FaceTarget(transform.position + fleeDirection);

        // Move to the best flee position with flee speed and force running animation
        MoveToPoint(bestFleePosition, fleeSpeed);
        UpdateAnimationState(false, false, true, false);  // Force running animation during fleeing

        // Play panic sounds
        if (movementSounds.Length > 0 && Time.time % 2 < 0.1f)
        {
            PlayRandomSound(movementSounds, 1.2f);
        }
    }

    private Vector3 FindBestFleePosition(Vector3 baseFleeDirection)
    {
        float[] angles = { 0, 30, -30, 60, -60 };
        Vector3 bestPosition = transform.position;
        float bestDistance = 0f;

        foreach (float angle in angles)
        {
            // Try different angles
            Vector3 testDirection = Quaternion.Euler(0, angle, 0) * baseFleeDirection;
            Vector3 targetPos = transform.position + testDirection * safeDistance;

            // Sample points between current position and target
            for (float distance = MIN_FLEE_DISTANCE; distance <= safeDistance; distance += 2f)
            {
                Vector3 testPosition = transform.position + testDirection * distance;

                if (NavMesh.SamplePosition(testPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
                {
                    float distToPlayer = Vector3.Distance(hit.position, player.position);
                    if (distToPlayer > bestDistance)
                    {
                        bestDistance = distToPlayer;
                        bestPosition = hit.position;
                    }
                }
            }
        }

        return bestPosition;
    }

    protected override void HandlePursuing()
    {
        ChangeState(EnemyState.Fleeing);
    }

    protected override void HandleAttacking()
    {
        ChangeState(EnemyState.Fleeing);
    }

    protected override void ChangeState(EnemyState newState)
    {
        if (newState == EnemyState.Fleeing)
        {
            StopMovement();
            currentSpeed = fleeSpeed;  // Set current speed to flee speed
            // Force running animation when fleeing
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", true);
            animator.SetBool("IsAttacking", false);
        }
        else if (currentState == EnemyState.Fleeing && newState == EnemyState.Patrolling)
        {
            // Keep the same forward direction when exiting flee state
            Vector3 currentForward = transform.forward;
            base.ChangeState(newState);
            transform.forward = currentForward;
            return;
        }
    
        base.ChangeState(newState);
    }

    public override void OnDamageReceived()
    {
        isPanicked = true;
        timeStartedFleeing = Time.time;
        ChangeState(EnemyState.Fleeing);
        
        if (player != null)
        {
            Vector3 awayFromPlayer = (transform.position - player.position).normalized;
            lastSafePosition = transform.position + awayFromPlayer * safeDistance;
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, panicDistance);

        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        if (lastSafePosition != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastSafePosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastSafePosition);
        }
    }
}