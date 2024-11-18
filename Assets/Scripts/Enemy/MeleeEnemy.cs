using UnityEngine;
using System.Collections;

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

    protected override void HandleAttacking()
    {
        // Stop moving when attacking
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Keep facing the player
        FaceTarget(player.position);

        // Only attack if enough time has passed since last attack
        if (Time.time >= lastAttackTime + attackInterval)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsAttacking", true);
            PerformAttack();
        }
    }

    protected override void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange + 1f)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }

        StartCoroutine(ResetAttackAnimation());
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