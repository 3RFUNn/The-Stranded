using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class CowardlyEnemy : BaseEnemy
{
    [Header("Cowardly Specific Settings")]
    [SerializeField] private float panicDistance = 8f;      // Distance at which enemy starts panicking
    [SerializeField] private float safeDistance = 15f;      // Distance at which enemy feels safe
    [SerializeField] private float fleeSpeed = 10f;         // Speed when fleeing
    [SerializeField] private float calmDownTime = 5f;       // Time needed to calm down when out of sight
    
    private bool isPanicked = false;
    private float timeStartedFleeing;
    private Vector3 lastSafePosition;
    private const float MIN_FLEE_DISTANCE = 5f;  // Minimum distance to move when fleeing

    protected override void Start()
    {
        base.Start();
        maxSpeed = fleeSpeed;
        runSpeed = fleeSpeed;
        lastSafePosition = transform.position;
    }

    protected override void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
        // Reset state if we were attacking or pursuing (cowardly enemies don't attack)
        if (currentState == EnemyState.Attacking || currentState == EnemyState.Pursuing)
        {
            ChangeState(EnemyState.Patrolling);
        }

        // Check for panic conditions
        if (canSeePlayer && distanceToPlayer <= panicDistance)
        {
            if (!isPanicked)
            {
                timeStartedFleeing = Time.time;
                isPanicked = true;
            }
            ChangeState(EnemyState.Fleeing);
            return;
        }

        // Handle fleeing state
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
            return;
        }

        // Default behavior when not panicked
        if (currentState != EnemyState.Fleeing)
        {
            base.UpdateState(distanceToPlayer, canSeePlayer);
        }
    }

    protected override void HandleFleeing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        // Calculate optimal flee direction
        Vector3 fleeDirection = GetOptimalFleeDirection();
        Vector3 targetPosition = transform.position + fleeDirection * safeDistance;

        // Find valid position on NavMesh
        if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, safeDistance, NavMesh.AllAreas))
        {
            // Only update destination if it's significantly different from current path
            if (Vector3.Distance(agent.destination, hit.position) > MIN_FLEE_DISTANCE)
            {
                MoveToPoint(hit.position, fleeSpeed);
            }

            // Always face away from the player while fleeing
            FaceTarget(transform.position + fleeDirection);
            
            // Update animation state to running
            UpdateAnimationState(false, false, true, false);

            // Play panic sounds occasionally
            if (movementSounds.Length > 0 && Time.time % 2 < 0.1f)
            {
                PlayRandomSound(movementSounds, 1.2f); // Slightly louder for panic effect
            }
        }
        else
        {
            // If we can't find a valid flee position, try to return to last safe position
            MoveToPoint(lastSafePosition, fleeSpeed);
        }
    }

    private Vector3 GetOptimalFleeDirection()
    {
        Vector3 awayFromPlayer = transform.position - player.position;
        awayFromPlayer.y = 0;
        
        // Try multiple directions to find the best escape route
        float[] angles = { 0, 45, -45, 90, -90 };
        Vector3 bestDirection = awayFromPlayer.normalized;
        float bestDistance = 0;

        foreach (float angle in angles)
        {
            Vector3 testDirection = Quaternion.Euler(0, angle, 0) * awayFromPlayer.normalized;
            Vector3 testPosition = transform.position + testDirection * safeDistance;

            // Check if position is on NavMesh
            if (NavMesh.SamplePosition(testPosition, out NavMeshHit hit, safeDistance, NavMesh.AllAreas))
            {
                float distanceFromPlayer = Vector3.Distance(hit.position, player.position);
                if (distanceFromPlayer > bestDistance)
                {
                    bestDistance = distanceFromPlayer;
                    bestDirection = testDirection;
                }
            }
        }

        return bestDirection;
    }

    protected override void HandlePursuing()
    {
        // Cowardly enemies don't pursue - change to fleeing instead
        ChangeState(EnemyState.Fleeing);
    }

    protected override void HandleAttacking()
    {
        // Cowardly enemies don't attack - change to fleeing instead
        ChangeState(EnemyState.Fleeing);
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw panic distance
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, panicDistance);

        // Draw safe distance
        Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, safeDistance);

        // Draw last safe position if set
        if (lastSafePosition != Vector3.zero)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(lastSafePosition, 0.5f);
            Gizmos.DrawLine(transform.position, lastSafePosition);
        }
    }

    public override void OnDamageReceived()
    {
        // Immediately panic when damaged
        isPanicked = true;
        timeStartedFleeing = Time.time;
        ChangeState(EnemyState.Fleeing);
        
        // Store player position for fleeing direction
        if (player != null)
        {
            Vector3 awayFromPlayer = transform.position - player.position;
            lastSafePosition = transform.position + awayFromPlayer.normalized * safeDistance;
        }
    }
}