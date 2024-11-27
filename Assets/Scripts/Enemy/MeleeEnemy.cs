using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class MeleeEnemy : BaseEnemy
{
    [Header("Melee Specific Settings")]
    [SerializeField] private float chargeSpeed = 8f;
    [SerializeField] private float minAttackCooldown = 1.5f;
    
    private bool isInAttackTrigger = false;
    private PlayerHealth playerHealth;

    protected override void Start()
    {
        base.Start();
        maxSpeed = chargeSpeed;
        runSpeed = chargeSpeed;
        attackInterval = minAttackCooldown;
    }

    protected override void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        // If we're in attack trigger, transition to attack state
        if (isInAttackTrigger)
        {
            ChangeState(EnemyState.Attacking);
            return;
        }

        // Charge at the player
        MoveToPoint(player.position, chargeSpeed);
        FaceTarget(player.position);
        UpdateAnimationState(false, false, true, false);

        // Play movement sounds
        if (movementSounds.Length > 0 && Time.time % 3 < 0.1f)
        {
            PlayRandomSound(movementSounds);
        }
    }

    protected override void HandleAttacking()
    {
        if (player == null || !isInAttackTrigger)
        {
            ChangeState(EnemyState.Pursuing);
            return;
        }

        // Stop all movement
        StopMovement();
        
        // Face the player while attacking
        FaceTarget(player.position);

        // Update animation state
        UpdateAnimationState(false, false, false, true);

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

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInAttackTrigger = true;
            if (playerHealth == null)
            {
                playerHealth = other.GetComponentInParent<PlayerHealth>();
            }
            
            if (currentState == EnemyState.Pursuing)
            {
                ChangeState(EnemyState.Attacking);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isInAttackTrigger = false;
            if (currentState == EnemyState.Attacking)
            {
                ChangeState(EnemyState.Pursuing);
            }
        }
    }

    private void PerformAttack()
    {
        // Play attack sound
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
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
    }
}