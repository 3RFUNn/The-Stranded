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

    [Header("Animation Settings")]
    [SerializeField] protected float animationBlendSpeed = 8f;
    protected float currentAnimationBlend = 0f;

    [Header("Audio")] 
    [SerializeField] protected AudioClip[] attackSounds;
    [SerializeField] protected AudioClip[] movementSounds;
    [SerializeField] protected AudioClip[] idleSounds;

    protected Transform player;
    protected EnemyState currentState;
    protected bool isTransitioningAnimation = false;

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
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
                }
                break;

            case EnemyState.Pursuing:
                if (distanceToPlayer <= attackRange)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Attacking));
                }
                else if (distanceToPlayer > detectionRange)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Patrolling));
                }
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange + 0.5f)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
                }
                break;

            case EnemyState.Fleeing:
                if (distanceToPlayer >= safeDistance)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Patrolling));
                }
                break;
        }
    }

    protected IEnumerator SmoothStateTransition(EnemyState newState)
    {
        if (isTransitioningAnimation)
            yield break;

        isTransitioningAnimation = true;

        // Smoothly blend out current animation
        float currentBlend = 1f;
        while (currentBlend > 0)
        {
            currentBlend -= Time.deltaTime * animationBlendSpeed;
            UpdateAnimationBlend(currentBlend);
            yield return null;
        }

        // Change state
        ChangeState(newState);

        // Smoothly blend in new animation
        currentBlend = 0f;
        while (currentBlend < 1)
        {
            currentBlend += Time.deltaTime * animationBlendSpeed;
            UpdateAnimationBlend(currentBlend);
            yield return null;
        }

        isTransitioningAnimation = false;
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
        
        if (distanceToPatrolPoint < 0.3f)
        {
            StopAndSnapToPosition(currentPatrolPoint);
            StartCoroutine(WaitAtPatrolPoint());
        }
        else
        {
            Vector3 directionToTarget = (currentPatrolPoint - transform.position).normalized;
            float angle = Vector3.Angle(transform.forward, directionToTarget);

            if (angle > 30f)
            {
                StopAndRotate(currentPatrolPoint);
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
        
        agent.isStopped = false;
        agent.SetDestination(player.position);
        UpdateAnimation("IsRunning", true);
    }

    protected virtual void HandleAttacking()
    {
        // Stop moving when attacking
        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        // Keep facing the player
        FaceTarget(player.position);

        // Only set attacking animation, no idle
        if (Time.time >= lastAttackTime + attackInterval)
        {
            animator.SetBool("IsIdle", false);
            animator.SetBool("IsAttacking", true);
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

    protected virtual void StopAndSnapToPosition(Vector3 position)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        transform.position = new Vector3(position.x, transform.position.y, position.z);
        UpdateAnimation("IsIdle", true);
    }

    protected virtual void StopAndRotate(Vector3 targetPosition)
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        FaceTarget(targetPosition);
        UpdateAnimation("IsIdle", true);
    }

    protected virtual void PerformAttack()
    {
        lastAttackTime = Time.time;
        
        if (attackSounds.Length > 0)
        {
            PlayRandomSound(attackSounds);
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            player.GetComponent<PlayerHealth>().TakeDamage(attackDamage);
        }

        // Start a coroutine to reset the attack animation after a delay
        StartCoroutine(ResetAttackAnimation());
    }

    protected virtual IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f); // Adjust this time to match your attack animation length
        if (currentState == EnemyState.Attacking)
        {
            animator.SetBool("IsAttacking", false);
        }
    }

    protected virtual void SetNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y; // Keep the same Y level

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
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

    protected virtual void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        directionToTarget.y = 0; // Keep vertical rotation locked
        
        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 1000f);
        }
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        currentState = newState;
        ResetAnimations();
    }

    protected virtual void UpdateAnimationBlend(float blend)
    {
        if (animator != null)
        {
            animator.SetLayerWeight(0, blend);
        }
    }

    protected virtual void UpdateAnimation(string parameterName, bool value)
    {
        if (animator != null && !isTransitioningAnimation)
        {
            ResetAnimations();
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