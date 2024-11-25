using UnityEngine;
using System.Collections;

public class MeleeEnemy : BaseEnemy
{
    #region Serialized Fields
    [Header("Melee Combat Settings")]
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 2f;
    [SerializeField] private float attackAnimationDuration = 1f;
    [SerializeField] private float rushSpeed = 8f;
    [SerializeField] private float rushThreshold = 5f;
    
    [Header("Combat Animations")]
    [SerializeField] private string attackTriggerName = "Attack";
    [SerializeField] private string attackBoolName = "IsAttacking";
    #endregion

    #region Private Fields
    private float lastAttackTime;
    private bool isAttacking;
    private bool isRushing;
    #endregion

    #region State Management
    protected override void UpdateState()
    {
        if (!IsPlayerValid()) return;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        bool canSeePlayer = IsPlayerInFieldOfView() && HasLineOfSightToPlayer();

        switch (CurrentState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrolling:
                if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Pursuing);
                }
                break;

            case EnemyState.Pursuing:
                if (distanceToPlayer <= attackRange && canSeePlayer)
                {
                    ChangeState(EnemyState.Attacking);
                }
                else if (!canSeePlayer && distanceToPlayer > detectionRange)
                {
                    ChangeState(EnemyState.Patrolling);
                }
                else
                {
                    // Update rushing state based on distance
                    isRushing = distanceToPlayer <= rushThreshold;
                }
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange || !canSeePlayer)
                {
                    ChangeState(EnemyState.Pursuing);
                }
                break;
        }
    }

    protected override void HandleCurrentState()
    {
        switch (CurrentState)
        {
            case EnemyState.Idle:
                HandleIdle();
                break;
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;
            case EnemyState.Pursuing:
                HandlePursuing();
                break;
            case EnemyState.Attacking:
                HandleAttacking();
                break;
        }
    }

    protected override void HandleStateTransition(EnemyState oldState, EnemyState newState)
    {
        base.HandleStateTransition(oldState, newState);
        
        // Reset combat flags on state change
        if (newState != EnemyState.Attacking)
        {
            isAttacking = false;
            isRushing = false;
        }
    }
    #endregion

    #region State Handlers
    private void HandleIdle()
    {
        StopAgent();
        PlayAnimation("IsIdle");
    }

    private void HandlePatrolling()
    {
        // Simple patrol behavior - can be expanded
        if (Agent.remainingDistance < 0.1f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 10f;
            MoveToPosition(randomPoint);
        }
        PlayAnimation("IsWalking");
    }

    private void HandlePursuing()
    {
        if (!IsPlayerValid()) return;

        // Set appropriate speed based on distance
        SetAgentSpeed(isRushing ? rushSpeed : runSpeed);
        
        // Move towards player
        MoveToPosition(Player.position);
        FaceTarget(Player.position);

        // Play movement sound occasionally
        if (Random.value < 0.01f)
        {
            PlaySound(movementSounds);
        }

        // Update animation
        PlayAnimation(isRushing ? "IsRunning" : "IsWalking");
    }

    private void HandleAttacking()
    {
        if (!IsPlayerValid() || isAttacking) return;

        StopAgent();
        FaceTarget(Player.position);

        if (Time.time >= lastAttackTime + attackInterval)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }
    #endregion

    #region Combat
    private IEnumerator PerformAttackSequence()
    {
        isAttacking = true;
        lastAttackTime = Time.time;

        // Start attack animation
        if (Animator != null)
        {
            Animator.SetTrigger(attackTriggerName);
            Animator.SetBool(attackBoolName, true);
        }

        // Play attack sound
        PlaySound(attackSounds);

        // Wait for the "impact" moment in the animation
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        // Apply damage if still in range
        if (IsPlayerValid() && IsInAttackRange())
        {
            ApplyDamage();
        }

        // Wait for animation to finish
        yield return new WaitForSeconds(attackAnimationDuration * 0.5f);

        // Reset attack state
        if (Animator != null)
        {
            Animator.SetBool(attackBoolName, false);
        }
        isAttacking = false;
    }

    private void ApplyDamage()
    {
        // Attempt to get and damage the player's health component
        if (Player.TryGetComponent<IDamageable>(out var damageable))
        {
            damageable.TakeDamage(attackDamage);
        }
    }

    private bool IsInAttackRange()
    {
        if (!IsPlayerValid()) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        return distanceToPlayer <= attackRange;
    }
    #endregion

    #region Debug
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw rush threshold
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rushThreshold);
    }
    #endregion
}

// Interface for damageable entities
public interface IDamageable
{
    void TakeDamage(float damage);
}