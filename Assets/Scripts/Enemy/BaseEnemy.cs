// BaseEnemy.cs
using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public abstract class BaseEnemy : MonoBehaviour
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
    private bool isInitialized = false;

     protected virtual void Awake()
    {
        // Try to find player immediately on Awake
        FindPlayer();
    }

    protected virtual void Start()
    {
        InitializeComponents();
        if (VerifyComponents())
        {
            ChangeState(EnemyState.Patrolling);
            SetNewPatrolPoint();
        }
        else
        {
            // If components aren't verified, disable the script
            enabled = false;
        }
    }

    protected virtual void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        // Try to find player again if not found in Awake
        if (player == null)
        {
            FindPlayer();
        }
    }

    private void FindPlayer()
    {
        if (player == null)
        {
            // First try finding by tag
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
                Debug.Log($"Player found by {gameObject.name}");
            }
            else
            {
                // If tag fails, try finding specific player component
                playerObj = GameObject.FindObjectOfType<PlayerHealth>()?.gameObject;
                if (playerObj != null)
                {
                    player = playerObj.transform;
                    Debug.Log($"Player found through PlayerHealth by {gameObject.name}");
                }
                else
                {
                    Debug.LogError($"Player not found by {gameObject.name}! Ensure player has 'Player' tag or PlayerHealth component!");
                }
            }
        }
    }

    protected virtual bool VerifyComponents()
    {
        bool isValid = true;

        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent missing on {gameObject.name}!");
            isValid = false;
        }

        if (player == null)
        {
            Debug.LogError($"Player reference not found for {gameObject.name}!");
            isValid = false;
        }

        return isValid;
    }

    protected virtual void Update()
    {
        // Add null check for player
        if (player == null)
        {
            FindPlayer(); // Try to find player again
            if (player == null) // If still null, skip update
            {
                return;
            }
        }

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

    // Made abstract to force implementation in derived classes
    protected abstract void HandleAttacking();

    // Made virtual with default fleeing behavior
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

    // Made abstract to force implementation in derived classes
    protected abstract void PerformAttack();

    protected virtual IEnumerator ResetAttackAnimation()
    {
        yield return new WaitForSeconds(0.5f);
        if (currentState == EnemyState.Attacking)
        {
            animator.SetBool("IsAttacking", false);
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

    protected virtual void SetNewPatrolPoint()
    {
        Vector3 randomDirection = Random.insideUnitSphere * patrolRadius;
        randomDirection += transform.position;
        randomDirection.y = transform.position.y;

        if (NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            currentPatrolPoint = hit.position;
        }
    }

    protected virtual IEnumerator WaitAtPatrolPoint()
    {
        isWaitingAtPatrolPoint = true;
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
        directionToTarget.y = 0;
        
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

    protected IEnumerator SmoothStateTransition(EnemyState newState)
    {
        if (isTransitioningAnimation)
            yield break;

        isTransitioningAnimation = true;

        float currentBlend = 1f;
        while (currentBlend > 0)
        {
            currentBlend -= Time.deltaTime * animationBlendSpeed;
            UpdateAnimationBlend(currentBlend);
            yield return null;
        }

        ChangeState(newState);

        currentBlend = 0f;
        while (currentBlend < 1)
        {
            currentBlend += Time.deltaTime * animationBlendSpeed;
            UpdateAnimationBlend(currentBlend);
            yield return null;
        }

        isTransitioningAnimation = false;
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