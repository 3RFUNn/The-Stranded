using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemy : BaseEnemy
{
    [Header("Melee Specific Settings")]
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float minAttackCooldown = 1.5f;
    
    private PlayerHealth playerHealth;

    protected override void Start()
    {
        base.Start();
        maxSpeed = chargeSpeed;
        runSpeed = chargeSpeed;
        attackInterval = minAttackCooldown;
        attackRange = 3f;
    }

    protected override void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Always face the player while pursuing
        FaceTarget(player.position);

        // Check if we're in attack range
        if (distanceToPlayer <= attackRange)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        // Continue pursuing
        MoveToPoint(player.position, chargeSpeed);
        UpdateAnimationState(false, false, true, false);

        // Play movement sounds
        if (movementSounds.Length > 0 && Time.time % 2 < 0.1f)
        {
            PlayRandomSound(movementSounds);
        }
    }

    protected override void HandleAttacking()
    {
        if (player == null)
        {
            ChangeState(EnemyState.Pursuing);
            return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        
        // Exit attack state if we're too far from the player
        if (distanceToPlayer > attackRange)
        {
            ChangeState(EnemyState.Pursuing);
            return;
        }

        // Stop all movement
        StopMovement();
        
        // Face the player while attacking
        FaceTarget(player.position);

        // Perform attack if cooldown is over
        if (Time.time >= lastAttackTime + attackInterval)
        {
            PerformAttack();
            lastAttackTime = Time.time;
        }
    }

    protected override void ChangeState(EnemyState newState)
    {
        // If entering attack state
        if (newState == EnemyState.Attacking)
        {
            StopMovement();
            UpdateAnimationState(false, false, false, true);
        }
        // If leaving attack state
        else if (currentState == EnemyState.Attacking)
        {
            UpdateAnimationState(true, false, false, false);
        }

        base.ChangeState(newState);
    }

    private void PerformAttack()
    {
        // Play attack sound
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds, canPlay: true);
        }

        // Get reference to player health if we don't have it
        if (playerHealth == null && player != null)
        {
            playerHealth = player.GetComponent<PlayerHealth>();
        }

        // Apply damage if player health exists
        if (playerHealth != null)
        {
            playerHealth.TakeDamage(attackDamage);
            Debug.Log($"Melee attack dealt {attackDamage} damage to player.");
        }
    }

    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();
        
        // Draw charge speed range
        Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);
        Gizmos.DrawWireSphere(transform.position, chargeSpeed);
        
        // Draw attack range
        Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}