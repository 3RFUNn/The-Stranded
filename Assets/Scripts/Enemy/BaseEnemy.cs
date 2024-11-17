// BaseEnemy.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class BaseEnemy : MonoBehaviour
{
    [Header("Components")] 
    protected NavMeshAgent agent;
    protected Animator animator;
    protected AudioSource audioSource;

    [Header("Detection Settings")] 
    [SerializeField] protected float detectionRange = 10f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float safeDistance = 15f;

    [Header("Combat Settings")] 
    [SerializeField] protected int attackDamage = 10;
    [SerializeField] protected float attackInterval = 2.0f;
    protected float lastAttackTime;

    [Header("Patrol Settings")] 
    [SerializeField] protected float patrolRadius = 20f;
    [SerializeField] protected float waitTimeAtPatrolPoint = 3f;
    protected Vector3 currentPatrolPoint;
    protected bool isWaitingAtPatrolPoint;

    [Header("Audio")] 
    [SerializeField] protected AudioClip[] attackSounds;
    [SerializeField] protected AudioClip[] movementSounds;
    [SerializeField] protected AudioClip[] idleSounds;

    protected Transform player;
    protected EnemyState currentState;

    protected virtual void Start()
    {
        // Initialize components
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.FindGameObjectWithTag("Player").transform;

        // Verify components
        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent missing on {gameObject.name}!");
            return;
        }

        if (player == null)
        {
            Debug.LogError("Player not found! Make sure it has the 'Player' tag.");
            return;
        }

        // Start with patrol state
        ChangeState(EnemyState.Patrolling);
        SetNewPatrolPoint();
    }

    protected virtual void Update()
    {
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        UpdateState(distanceToPlayer);
        HandleCurrentState();
    }
    
   

    protected virtual void UpdateState(float distanceToPlayer)
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                if (distanceToPlayer <= detectionRange)
                    ChangeState(EnemyState.Pursuing);
                break;

            case EnemyState.Pursuing:
                if (distanceToPlayer <= attackRange)
                    ChangeState(EnemyState.Attacking);
                else if (distanceToPlayer > detectionRange)
                    ChangeState(EnemyState.Patrolling);
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                    ChangeState(EnemyState.Pursuing);
                break;

            case EnemyState.Fleeing:
                if (distanceToPlayer >= safeDistance)
                    ChangeState(EnemyState.Patrolling);
                break;
        }
    }

    protected virtual void HandleCurrentState()
    {
        switch (currentState)
        {
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;

            case EnemyState.Pursuing:
                HandlePursuing();
                break;

            case EnemyState.Attacking:
                HandleAttacking();
                break;

            case EnemyState.Fleeing:
                HandleFleeing();
                break;
        }
    }

    protected virtual void HandlePatrolling()
    {
        if (isWaitingAtPatrolPoint)
            return;

        float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolPoint);
        
        // Use a smaller threshold for stopping
        if (distanceToPatrolPoint < 0.3f)  // Reduced from 1f to 0.3f
        {
            // Immediately stop the agent and snap to position
            agent.isStopped = true;
            agent.velocity = Vector3.zero;  // Reset velocity to prevent sliding
            
            // Optional: Snap to exact patrol point to prevent tiny movements
            transform.position = new Vector3(
                currentPatrolPoint.x,
                transform.position.y,
                currentPatrolPoint.z
            );
            
            StartCoroutine(WaitAtPatrolPoint());
        }
        else
        {
            Vector3 directionToTarget = (currentPatrolPoint - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle > 30f)
            {
                agent.isStopped = true;
                agent.velocity = Vector3.zero;  // Reset velocity when stopping to rotate
                FaceTarget(currentPatrolPoint);
                UpdateAnimation("IsIdle", true);
            }
            else
            {
                agent.isStopped = false;
                agent.SetDestination(currentPatrolPoint);
                UpdateAnimation("IsWalking", true);
            }
        }
    }

    protected virtual void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null) return;
        
        agent.SetDestination(player.position);
        UpdateAnimation("IsRunning", true);
    }

    protected virtual void HandleAttacking()
    {
        FaceTarget(player.position);
        if (Time.time >= lastAttackTime + attackInterval)
        {
            PerformAttack();
        }
    }

    protected virtual void HandleFleeing()
    {
        Vector3 fleeDirection = transform.position - player.position;
        Vector3 fleePosition = transform.position + fleeDirection.normalized * safeDistance;
        
        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            FaceTarget(transform.position + fleeDirection);
            UpdateAnimation("IsRunning", true);
        }
    }

    protected virtual void PerformAttack()
    {
        lastAttackTime = Time.time;
        UpdateAnimation("IsAttacking", true);
        
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }

        if (Vector3.Distance(transform.position, player.position) <= attackRange)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }
    }

    protected virtual void SetNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        NavMeshHit hit;

        if (NavMesh.SamplePosition(randomDirection, out hit, patrolRadius, NavMesh.AllAreas))
        {
            currentPatrolPoint = hit.position;
        }
    }

    protected virtual IEnumerator WaitAtPatrolPoint()
    {
        isWaitingAtPatrolPoint = true;
        
        // Ensure the agent is fully stopped
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        
        UpdateAnimation("IsIdle", true);
        
        if (idleSounds.Length > 0)
        {
            PlayRandomSound(idleSounds);
        }

        yield return new WaitForSeconds(waitTimeAtPatrolPoint);
        
        SetNewPatrolPoint();
        isWaitingAtPatrolPoint = false;
    }

    // Option 2: Faster Slerp (very quick but still smooth)
    protected virtual void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        directionToTarget.y = 0;
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 1000f);
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        currentState = newState;
        ResetAnimations();
    }

    protected virtual void UpdateAnimation(string parameterName, bool value)
    {
        if (animator != null)
        {
            // First reset all animations
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);

            // Then set the desired animation
            animator.SetBool(parameterName, value);
        }
    }

    protected virtual void ResetAnimations()
    {
        if (animator != null)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsRunning", false);
            animator.SetBool("IsAttacking", false);
        }
    }

    protected virtual void PlayRandomSound(AudioClip[] sounds)
    {
        if (sounds.Length > 0 && audioSource != null)
        {
            AudioClip randomSound = sounds[Random.Range(0, sounds.Length)];
            audioSource.PlayOneShot(randomSound);
        }
    }
}