using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : BaseEnemy
{
    [Header("Melee Specific Settings")]
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float normalSpeed = 3.5f;

    protected override void Start()
    {
        base.Start();
        agent.speed = normalSpeed;
    }

    protected override void HandlePursuing()
    {
        agent.speed = chargeSpeed;
        base.HandlePursuing();
    }

    protected override void ChangeState(EnemyState newState)
    {
        if (newState != EnemyState.Pursuing)
        {
            agent.speed = normalSpeed;
        }
        base.ChangeState(newState);
    }
}
