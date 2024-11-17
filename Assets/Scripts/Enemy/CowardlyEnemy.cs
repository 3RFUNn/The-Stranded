using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CowardlyEnemy : BaseEnemy
{
    [Header("Cowardly Specific Settings")]
    [SerializeField] private float fleeThreshold = 5f;
    [SerializeField] private float fleeSpeed = 7f;

    protected override void UpdateState(float distanceToPlayer)
    {
        if (distanceToPlayer <= fleeThreshold && currentState != EnemyState.Fleeing)
        {
            ChangeState(EnemyState.Fleeing);
            agent.speed = fleeSpeed;
        }
        else
        {
            base.UpdateState(distanceToPlayer);
        }
    }

    protected override void ChangeState(EnemyState newState)
    {
        if (newState != EnemyState.Fleeing)
        {
            agent.speed = agent.speed / 2; // Return to normal speed
        }
        base.ChangeState(newState);
    }
}
