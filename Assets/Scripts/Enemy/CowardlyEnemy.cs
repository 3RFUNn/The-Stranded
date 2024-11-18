using UnityEngine;
using UnityEngine.AI;

public class CowardlyEnemy : BaseEnemy
{
    [Header("Cowardly Specific Settings")]
    [SerializeField] private float fleeThreshold = 5f;
    [SerializeField] private float fleeSpeed = 7f;
    [SerializeField] private float normalSpeed = 3f;

    protected override void Start()
    {
        base.Start();
        agent.speed = normalSpeed;
    }

    protected override void UpdateState(float distanceToPlayer)
    {
        // Completely override base behavior
        switch (currentState)
        {
            case EnemyState.Patrolling:
                // If player gets within detection range, immediately flee
                if (distanceToPlayer <= fleeThreshold)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
                    agent.speed = fleeSpeed;
                }
                break;

            case EnemyState.Fleeing:
                // Return to patrolling only when we're at a safe distance
                if (distanceToPlayer >= safeDistance)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Patrolling));
                    agent.speed = normalSpeed;
                }
                break;

            // If somehow ended up in these states, switch to fleeing
            case EnemyState.Pursuing:
            case EnemyState.Attacking:
                StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
                agent.speed = fleeSpeed;
                break;
        }
    }

    // Override and prevent base behavior to ensure no attacking
    protected override void HandleAttacking()
    {
        // Immediately switch to fleeing
        StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
    }

    // Override and prevent base behavior to ensure no attacking
    protected override void PerformAttack()
    {
        // No attack behavior for cowardly enemy
    }

    protected override void HandleFleeing()
    {
        // Calculate the direction away from the player
        Vector3 fleeDirection = transform.position - player.position;
        Vector3 fleePosition = transform.position + fleeDirection.normalized * safeDistance;
        
        NavMeshHit hit;
        // Try to find a valid position to flee to
        if (NavMesh.SamplePosition(fleePosition, out hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            // Always face away from the player while fleeing
            Vector3 lookPosition = transform.position + fleeDirection;
            FaceTarget(lookPosition);
            UpdateAnimation("IsRunning", true);
        }
    }

    protected override void HandlePursuing()
    {
        // Override to prevent any pursuit behavior
        StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
    }

    protected override void ChangeState(EnemyState newState)
    {
        // Only allow Patrolling or Fleeing states
        if (newState != EnemyState.Fleeing && newState != EnemyState.Patrolling)
        {
            newState = EnemyState.Fleeing;
        }

        // Update speed based on state
        agent.speed = (newState == EnemyState.Fleeing) ? fleeSpeed : normalSpeed;
        
        base.ChangeState(newState);
    }
}