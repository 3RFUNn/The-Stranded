using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class MeleeEnemy : BaseEnemy
{
    [Header("Melee Specific Settings")]
    [SerializeField] private float rushSpeed = 10f;
    [SerializeField] private float rushThreshold = 5f;
    [SerializeField] private float stoppingDistance = 1.5f;
    private bool isRushing = false;
    private bool isAttackingAnimation = false;

    protected override void Start()
    {
        base.Start();
        maxSpeed = rushSpeed;
        attackInterval = 5f; // Set attack interval to 5 seconds
        Debug.Log($"MeleeEnemy initialized with attack interval: {attackInterval} seconds");
    }

    protected override void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null)
        {
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if we're close enough to attack
        if (distanceToPlayer <= attackRange + 0.5f)
        {
            isAttackingAnimation = false; // Reset attack animation flag
            ChangeState(EnemyState.Attacking);
            return;
        }
        
        // Start rushing when within threshold
        if (distanceToPlayer <= rushThreshold && !isRushing)
        {
            isRushing = true;
            currentSpeed = maxSpeed;
        }
        
        agent.stoppingDistance = stoppingDistance;
        agent.isStopped = false;
        agent.SetDestination(player.position);
        FaceTarget(player.position);
        
        // Keep the rushing state until we either get too far or too close
        if (isRushing && (distanceToPlayer > rushThreshold * 1.5f || distanceToPlayer <= attackRange))
        {
            isRushing = false;
        }

        // Update animation based on movement
        bool isMovingFast = agent.velocity.magnitude > walkSpeed;
        UpdateAnimation(isMovingFast ? "IsRunning" : "IsWalking", true);
    }

    protected override void HandleAttacking()
    {
        if (player == null)
        {
            return;
        }

        // Stop moving when attacking
        if (agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // Keep facing the player
        FaceTarget(player.position);

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Check if we should exit attack state
        if (distanceToPlayer > attackRange + 1f)
        {
            ResetAttackState();
            ChangeState(EnemyState.Pursuing);
            return;
        }

        // Check if enough time has passed for next attack
        if (Time.time >= lastAttackTime + attackInterval && distanceToPlayer <= attackRange + 0.5f)
        {
            StartAttackAnimation();
            PerformAttack();
            lastAttackTime = Time.time;
            Debug.Log($"Performing attack at time: {Time.time}");
        }
    }

    private void StartAttackAnimation()
    {
        if (animator != null)
        {
            ResetAnimations();
            isAttackingAnimation = true;
            animator.SetBool("IsAttacking", true);
            animator.SetTrigger("Attack");
        }
    }

    private void ResetAttackState()
    {
        isAttackingAnimation = false;
        if (animator != null)
        {
            animator.SetBool("IsAttacking", false);
            animator.ResetTrigger("Attack");
        }
    }

    protected override void PerformAttack()
    {
        // Play attack sound
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }

        // Get current distance to player
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        // Apply damage if in range
        if (distanceToPlayer <= attackRange + 0.5f)
        {
            var playerHealth = player.GetComponent<PlayerHealth>();
            Debug.Log($"PlayerHealth component found: {playerHealth != null}, Current Health: {playerHealth.CurrentHealth}");
            
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);
                Debug.Log($"Dealing {attackDamage} damage to player. New Health: {playerHealth.CurrentHealth}");
            }
        }

        // Start the animation reset coroutine
        StartCoroutine(ResetAttackAnimation());
    }

    protected override IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        if (currentState == EnemyState.Attacking)
        {
            ResetAttackState();
        }
    }

    protected override void HandlePatrolling()
    {
        isRushing = false;
        isAttackingAnimation = false;
        base.HandlePatrolling();
    }

    protected override void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
        // Reset rushing state when losing sight of player
        if (!canSeePlayer && isRushing)
        {
            isRushing = false;
        }
        
        base.UpdateState(distanceToPlayer, canSeePlayer);
    }

    protected override void ChangeState(EnemyState newState)
    {
        // Reset attack state when changing states
        if (newState != EnemyState.Attacking)
        {
            ResetAttackState();
        }
        
        base.ChangeState(newState);
    }

    protected override void UpdateSpeedAndAnimation()
    {
        // Don't update animations if we're in attack animation
        if (isAttackingAnimation)
        {
            return;
        }

        base.UpdateSpeedAndAnimation();
    }

    protected override void ResetAnimations()
    {
        if (animator != null && !isAttackingAnimation)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw rush threshold
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, rushThreshold);
    }
}