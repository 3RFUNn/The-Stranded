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
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] protected float attackRange = 2f;
    [SerializeField] protected float safeDistance = 15f;
    [SerializeField] protected float fieldOfViewAngle = 180f;

    [Header("Speed Settings")]
    [SerializeField] protected float walkSpeed = 2f;
    [SerializeField] protected float runSpeed = 5f;
    [SerializeField] protected float maxSpeed = 8f;
    [SerializeField] protected float speedAcceleration = 2f;
    protected float currentSpeed;

    [Header("Combat Settings")] 
    [SerializeField] protected int attackDamage = 10;
    [SerializeField] protected float attackInterval = 2.0f;
    protected float lastAttackTime;

    [Header("Patrol Settings")] 
    [SerializeField] protected float patrolRadius = 20f;
    [SerializeField] protected float waitTimeAtPatrolPoint = 3f;
    [SerializeField] protected float minPatrolDistance = 5f; // Minimum distance for new patrol point
    protected Vector3 currentPatrolPoint;
    protected bool isWaitingAtPatrolPoint;
    protected Vector3 lastKnownPlayerPosition;
    protected Quaternion lastPatrolRotation;
    protected float patrolWaitEndTime;

    [Header("Animation Settings")]
    [SerializeField] protected float animationBlendSpeed = 8f;
    [SerializeField] protected float rotationSpeed = 5f;
    protected float currentAnimationBlend = 0f;

    [Header("Audio")] 
    [SerializeField] protected AudioClip[] attackSounds;
    [SerializeField] protected AudioClip[] movementSounds;
    [SerializeField] protected AudioClip[] idleSounds;

    protected Transform player;
    protected EnemyState currentState;
    protected bool isTransitioningAnimation = false;
    protected bool hasSpottedPlayer = false;
    protected bool isMoving = false;

    protected virtual void Start()
    {
        InitializeComponents();
        if (VerifyComponents())
        {
            currentSpeed = 0;
            ChangeState(EnemyState.Idle);
            StartCoroutine(DelayedPatrolStart());
        }
        else
        {
            enabled = false;
        }
    }

    protected bool CheckLineOfSight()
    {
        if (player == null) return false;

        Vector3 directionToPlayer = (player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        if (angle <= fieldOfViewAngle * 0.5f && distanceToPlayer <= detectionRange)
        {
            lastKnownPlayerPosition = player.position;
            return true;
        }
        return false;
    }

    protected virtual void HandleIdle()
    {
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        isMoving = false;
        UpdateAnimation("IsIdle", true);

        if (Time.time >= patrolWaitEndTime)
        {
            Debug.Log($"Exiting Idle state at time: {Time.time}");
            SetNewPatrolPoint();
            ChangeState(EnemyState.Patrolling);
        }
    }

    protected virtual void StartWaitAtPatrolPoint()
    {
        isWaitingAtPatrolPoint = true;
        patrolWaitEndTime = Time.time + waitTimeAtPatrolPoint;
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        isMoving = false;
        
        // Changed to switch to Idle state after setting the wait time
        ChangeState(EnemyState.Idle);
        Debug.Log($"Started waiting at patrol point. Will resume at: {patrolWaitEndTime}");
        
        if (idleSounds.Length > 0)
        {
            PlayRandomSound(idleSounds);
        }
    }

    protected virtual void HandlePatrolling()
    {
        if (isWaitingAtPatrolPoint)
        {
            if (Time.time >= patrolWaitEndTime)
            {
                Debug.Log("Resuming patrol from wait point");
                isWaitingAtPatrolPoint = false;
                SetNewPatrolPoint();
            }
            return;
        }

        if (!agent.isOnNavMesh) return;

        float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolPoint);
        Debug.Log($"Distance to patrol point: {distanceToPatrolPoint}");
        
        if (distanceToPatrolPoint < 0.5f)
        {
            StartWaitAtPatrolPoint();
        }
        else
        {
            MoveToPatrolPoint();
        }
    }
    
    protected IEnumerator DelayedPatrolStart()
    {
        // Wait for initial setup
        yield return new WaitForSeconds(1f);

        // Make sure we're still in a valid state
        if (enabled && gameObject.activeInHierarchy)
        {
            // Set initial patrol point
            SetNewPatrolPoint();
            
            // Start in idle state first
            ChangeState(EnemyState.Idle);
            
            // Set the initial patrol wait time
            patrolWaitEndTime = Time.time + waitTimeAtPatrolPoint;
            
            // Reset movement flags
            isMoving = false;
            isWaitingAtPatrolPoint = true;
            
            // Update animations
            if (animator != null)
            {
                ResetAnimations();
                animator.SetBool("IsIdle", true);
            }
        }
    }

    private void MoveToPatrolPoint()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 directionToTarget = (currentPatrolPoint - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToTarget);
        float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolPoint);

        if (distanceToPatrolPoint < 1f)  // Changed from 0.5f to 1f
        {
            StartWaitAtPatrolPoint();
        }
        else if (angle > 30f)
        {
            StopAndRotate(currentPatrolPoint);
            isMoving = false;
        }
        else
        {
            agent.isStopped = false;
            agent.SetDestination(currentPatrolPoint);
            isMoving = true;
            UpdateAnimation("IsWalking", true);
        }
    }

    protected virtual void SetNewPatrolPoint()
    {
        int maxAttempts = 30;
        int attempts = 0;
        Vector3 newPoint;

        do
        {
            newPoint = transform.position + Random.insideUnitSphere * patrolRadius;
            newPoint.y = transform.position.y;
            attempts++;

            float distanceToNew = Vector3.Distance(transform.position, newPoint);
            if (distanceToNew >= minPatrolDistance && NavMesh.SamplePosition(newPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                currentPatrolPoint = hit.position;
                break;
            }
        } while (attempts < maxAttempts);

        if (attempts >= maxAttempts)
        {
            // If we couldn't find a good point, just use the current position
            currentPatrolPoint = transform.position;
            StartWaitAtPatrolPoint();
        }
    }

   protected virtual void UpdateSpeedAndAnimation()
    {
        // Don't update animations if we're in attacking state
        if (currentState == EnemyState.Attacking)
        {
            return;
        }

        float targetSpeed = currentState switch
        {
            EnemyState.Pursuing => hasSpottedPlayer ? maxSpeed : runSpeed,
            EnemyState.Patrolling => walkSpeed,
            EnemyState.Fleeing => maxSpeed,
            _ => 0f
        };

        currentSpeed = Mathf.Lerp(currentSpeed, targetSpeed, Time.deltaTime * speedAcceleration);
        
        if (agent != null && agent.isOnNavMesh)
        {
            agent.speed = currentSpeed;
            // Check if the agent is actually moving by checking its velocity magnitude
            isMoving = agent.velocity.magnitude > 0.1f;
        }

        // Update animation states based on movement and actual velocity
        if (animator != null)
        {
            bool shouldBeIdle = !isMoving || agent.velocity.magnitude < 0.1f;
            bool shouldBeWalking = isMoving && currentSpeed <= walkSpeed * 1.5f && agent.velocity.magnitude > 0.1f;
            bool shouldBeRunning = isMoving && currentSpeed > walkSpeed * 1.5f && agent.velocity.magnitude > 0.1f;

            // Don't update animations if we're already in a special state
            bool isInSpecialAnimation = animator.GetBool("IsAttacking");
            if (!isInSpecialAnimation)
            {
                // Only update animations if there's an actual change to prevent animation flickering
                if (animator.GetBool("IsIdle") != shouldBeIdle ||
                    animator.GetBool("IsWalking") != shouldBeWalking ||
                    animator.GetBool("IsRunning") != shouldBeRunning)
                {
                    ResetAnimations();
                    animator.SetBool("IsIdle", shouldBeIdle);
                    animator.SetBool("IsWalking", shouldBeWalking);
                    animator.SetBool("IsRunning", shouldBeRunning);
                }
            }
        }
    }

    protected virtual void UpdateAnimation(string parameterName, bool value)
    {
        if (animator != null && !isTransitioningAnimation)
        {
            ResetAnimations();
            animator.SetBool(parameterName, value);

            // Update isMoving flag based on animation state
            isMoving = parameterName == "IsWalking" || parameterName == "IsRunning";
        }
    }

    protected virtual void Update()
    {
        if (player == null)
        {
            FindPlayer();
            if (player == null) return;
        }

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CheckLineOfSight();
        
        UpdateState(distanceToPlayer, canSeePlayer);
        HandleCurrentState();
        UpdateSpeedAndAnimation();
    }

    protected virtual void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
        switch (currentState)
        {
            case EnemyState.Idle:
                if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    hasSpottedPlayer = true;
                    StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
                }
                break;

            case EnemyState.Patrolling:
                if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    hasSpottedPlayer = true;
                    StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
                }
                break;

            case EnemyState.Pursuing:
                if (distanceToPlayer <= attackRange && canSeePlayer)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Attacking));
                }
                else if (!canSeePlayer && distanceToPlayer > detectionRange)
                {
                    hasSpottedPlayer = false;
                    lastPatrolRotation = transform.rotation;
                    StartCoroutine(SmoothStateTransition(EnemyState.Patrolling));
                }
                break;

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange + 0.5f || !canSeePlayer)
                {
                    StartCoroutine(SmoothStateTransition(EnemyState.Pursuing));
                }
                break;

            case EnemyState.Fleeing:
                if (distanceToPlayer >= safeDistance && !canSeePlayer)
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

            case EnemyState.Fleeing:
                HandleFleeing();
                break;
        }
    }

    protected virtual void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null) return;
        
        agent.isStopped = false;
        agent.SetDestination(player.position);
        FaceTarget(player.position);
        isMoving = true;
        UpdateAnimation("IsRunning", true);

        if (Time.time % 3 < 0.1f && movementSounds.Length > 0)
        {
            PlayRandomSound(movementSounds);
        }
    }

    protected abstract void HandleAttacking();

    protected virtual void HandleFleeing()
    {
        if (!agent.isOnNavMesh) return;

        Vector3 fleeDirection = transform.position - player.position;
        Vector3 fleePosition = transform.position + fleeDirection.normalized * safeDistance;
        
        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, safeDistance, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            FaceTarget(transform.position + fleeDirection);
            isMoving = true;
            UpdateAnimation("IsRunning", true);
        }
    }

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
        if (!agent.isOnNavMesh) return;
        
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        transform.position = new Vector3(position.x, transform.position.y, position.z);
        isMoving = false;
        UpdateAnimation("IsIdle", true);
    }

    protected virtual void StopAndRotate(Vector3 targetPosition)
    {
        if (!agent.isOnNavMesh) return;
        
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        FaceTarget(targetPosition);
        isMoving = false;
        UpdateAnimation("IsIdle", true);
    }

    protected virtual void FaceTarget(Vector3 target)
    {
        Vector3 directionToTarget = (target - transform.position).normalized;
        directionToTarget.y = 0;
        
        if (directionToTarget != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

        // Store rotation if leaving patrol state
        if (currentState == EnemyState.Patrolling)
        {
            lastPatrolRotation = transform.rotation;
        }

        currentState = newState;
        ResetAnimations();

        // Reset agent properties on state change
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.velocity = Vector3.zero;
        }
        
        // Handle specific state change logic
        switch (newState)
        {
            case EnemyState.Idle:
                currentSpeed = 0f;
                isMoving = false;
                break;

            case EnemyState.Patrolling:
                currentSpeed = walkSpeed;
                if (!isWaitingAtPatrolPoint)
                {
                    SetNewPatrolPoint();
                }
                break;

            case EnemyState.Pursuing:
                currentSpeed = runSpeed;
                isMoving = true;
                break;

            case EnemyState.Attacking:
                if (agent != null && agent.isOnNavMesh)
                {
                    agent.isStopped = true;
                }
                currentSpeed = 0f;
                isMoving = false;
                break;

            case EnemyState.Fleeing:
                currentSpeed = maxSpeed;
                isMoving = true;
                break;
        }
    }

    protected IEnumerator SmoothStateTransition(EnemyState newState)
    {
        if (isTransitioningAnimation) yield break;

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
            currentAnimationBlend = blend;
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

    protected virtual void PlayRandomSound(AudioClip[] sounds, float volumeMultiplier = 1f)
    {
        if (sounds.Length > 0 && audioSource != null && !audioSource.isPlaying)
        {
            AudioClip randomSound = sounds[Random.Range(0, sounds.Length)];
            audioSource.volume = volumeMultiplier;
            audioSource.PlayOneShot(randomSound);
        }
    }

    protected void FindPlayer()
    {
        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                playerObj = GameObject.FindObjectOfType<PlayerHealth>()?.gameObject;
                if (playerObj != null)
                {
                    player = playerObj.transform;
                }
                else
                {
                    Debug.LogWarning($"Player not found by {gameObject.name}!");
                }
            }
        }
    }

    protected virtual void OnDrawGizmosSelected()
    {
        // Draw detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Draw attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Draw field of view
        Gizmos.color = Color.green;
        float halfFOV = fieldOfViewAngle * 0.5f;
        Vector3 rightDir = Quaternion.Euler(0, halfFOV, 0) * transform.forward;
        Vector3 leftDir = Quaternion.Euler(0, -halfFOV, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);

        // Draw current patrol point
        if (currentPatrolPoint != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentPatrolPoint, 0.5f);
            Gizmos.DrawLine(transform.position, currentPatrolPoint);
        }
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }
        ResetAnimations();
    }

    public virtual void OnDamageReceived()
    {
        if (player != null)
        {
            lastKnownPlayerPosition = player.position;
            hasSpottedPlayer = true;
        }
    }

    protected virtual void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        
        if (player == null)
        {
            FindPlayer();
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

        if (animator == null)
        {
            Debug.LogWarning($"Animator missing on {gameObject.name}!");
        }

        if (audioSource == null)
        {
            Debug.LogWarning($"AudioSource missing on {gameObject.name}!");
        }

        return isValid;
    }
}