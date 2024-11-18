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
        // Override the normal state transitions if player is too close
        if (distanceToPlayer <= fleeThreshold)
        {
            if (currentState != EnemyState.Fleeing)
            {
                StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
                agent.speed = fleeSpeed;
            }
        }
        else
        {
            // Only use base behavior if we're not too close to player
            base.UpdateState(distanceToPlayer);
        }
    }

    // Override and leave empty to prevent any attack behavior
    protected override void HandleAttacking()
    {
        // Immediately switch to fleeing if we somehow end up in attack state
        StartCoroutine(SmoothStateTransition(EnemyState.Fleeing));
    }

    // Override and leave empty to prevent any attack behavior
    protected override void PerformAttack()
    {
        // No attack behavior for cowardly enemy
    }

    protected override void HandleFleeing()
    {
        Vector3 fleeDirection = transform.position - player.position;
        Vector3 fleePosition = transform.position + fleeDirection.normalized * safeDistance;
        
        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            FaceTarget(transform.position + fleeDirection); // Face away from player while fleeing
            UpdateAnimation("IsRunning", true);
        }
    }

    protected override void ChangeState(EnemyState newState)
    {
        if (newState != EnemyState.Fleeing)
        {
            agent.speed = normalSpeed;
        }
        base.ChangeState(newState);
    }
}