using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public abstract class BaseEnemy : MonoBehaviour
{
    [Header("Components")] 
    protected NavMeshAgent agent;
    protected Animator animator;
    protected AudioSource audioSource;

    [Header("Detection Settings")] 
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] protected float attackRange = 4f;
    [SerializeField] protected float fieldOfViewAngle = 180f;

    [Header("Movement Settings")]
    [SerializeField] protected float walkSpeed = 2f;
    [SerializeField] protected float runSpeed = 5f;
    [SerializeField] protected float maxSpeed = 8f;
    [SerializeField] protected float rotationSpeed = 5f;
    protected float currentSpeed;

    [Header("Combat Settings")] 
    [SerializeField] protected int attackDamage = 10;
    [SerializeField] protected float attackInterval = 2.0f;
    protected float lastAttackTime;

    [Header("Patrol Settings")] 
    [SerializeField] protected float patrolRadius = 20f;
    [SerializeField] protected float waitTimeAtPatrolPoint = 3f;
    [SerializeField] protected float minPatrolDistance = 5f;
    protected Vector3 currentPatrolPoint;
    protected bool isWaitingAtPatrolPoint;
    protected float patrolWaitEndTime;

    [Header("Navigation Settings")]
    [SerializeField] protected float stuckCheckInterval = 0.5f;
    [SerializeField] protected float minMovementThreshold = 0.1f;
    [SerializeField] protected int stuckThreshold = 3;
    [SerializeField] protected float avoidancePriority = 50;
    [SerializeField] protected float avoidanceRadius = 1f;
    [SerializeField] protected float separationDistance = 2f;
    protected Vector3 lastPosition;
    protected int stuckCounter;
    protected float lastStuckCheck;
    protected bool isStuck;

    [Header("Animation Settings")]
    [SerializeField] protected float movementThreshold = 0.15f;
    [SerializeField] protected float animationSmoothTime = 0.2f;
    protected float attackAnimationDuration = 1.6f; // New field for attack animation
    protected bool isPerformingAttack = false; // New field to track attack state
    protected float currentAttackTime = 0f; // New field to track attack duration
    

    [Header("Audio")] 
    [SerializeField] protected AudioClip[] attackSounds;
    [SerializeField] protected AudioClip[] movementSounds;
    [SerializeField] protected AudioClip[] idleSounds;

    protected Transform player;
    protected EnemyState currentState;
    protected bool hasSpottedPlayer = false;
    protected bool isMoving = false;


    public int attackDamage1
    {
        get => attackDamage;
        set => attackDamage = value;
    }

    #region Initialization

    protected virtual void Start()
    {
        InitializeComponents();
        if (VerifyComponents())
        {
            currentSpeed = 0;
            lastPosition = transform.position;
            lastStuckCheck = Time.time;
            ChangeState(EnemyState.Idle);
            StartCoroutine(InitializePatrol());
        }
        else
        {
            enabled = false;
        }
    }

    protected virtual void InitializeComponents()
    {
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        FindPlayer();

        if (agent != null)
        {
            // Initialize NavMeshAgent parameters
            agent.acceleration = walkSpeed * 2;
            agent.angularSpeed = rotationSpeed * 100;
            agent.autoBraking = true;
            agent.autoRepath = true;
            agent.radius = avoidanceRadius;
            
            // Enhanced avoidance settings
            agent.avoidancePriority = Random.Range((int)avoidancePriority - 10, (int)avoidancePriority + 10);
            agent.stoppingDistance = 0.5f;
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            agent.baseOffset = 0.5f;
        }
    }

    protected virtual bool VerifyComponents()
    {
        if (agent == null)
        {
            Debug.LogError($"NavMeshAgent missing on {gameObject.name}!");
            return false;
        }
        
        if (animator == null || audioSource == null)
        {
            Debug.LogWarning($"Animator or AudioSource missing on {gameObject.name}!");
        }

        return true;
    }

    #endregion

    #region Core Update Logic

    protected virtual void Update()
    {
        if (player == null)
        {
            FindPlayer();
            return;
        }

        CheckIfStuck();
        EnforceEnemySeparation();
        
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CheckLineOfSight();
        
        UpdateState(distanceToPlayer, canSeePlayer);
        HandleCurrentState();
        UpdateAnimation();
    }

    protected virtual void CheckIfStuck()
    {
        if (Time.time - lastStuckCheck < stuckCheckInterval) return;

        lastStuckCheck = Time.time;

        if (!isMoving || !agent.isOnNavMesh)
        {
            stuckCounter = 0;
            isStuck = false;
            return;
        }

        float movement = Vector3.Distance(transform.position, lastPosition);
        Vector3 desiredVelocity = agent.desiredVelocity;
        
        if (movement < minMovementThreshold && desiredVelocity.magnitude > 0.1f)
        {
            if (Physics.Raycast(transform.position, desiredVelocity.normalized, out RaycastHit hit, agent.radius * 2))
            {
                stuckCounter++;
                if (stuckCounter >= stuckThreshold)
                {
                    HandleStuckState();
                }
            }
        }
        else
        {
            stuckCounter = 0;
            isStuck = false;
        }

        lastPosition = transform.position;
    }

    protected virtual void HandleStuckState()
    {
        if (isStuck) return;
        
        isStuck = true;
        StartCoroutine(UnstuckRoutine());
        
        if (currentState == EnemyState.Patrolling)
        {
            SetNewPatrolPoint();
        }
    }

    protected virtual IEnumerator UnstuckRoutine()
    {
        EnemyState previousState = currentState;
        Vector3 originalDestination = agent.destination;

        StopMovement();
        
        yield return new WaitForSeconds(0.2f);

        for (int i = 0; i < 8; i++)
        {
            float angle = i * 45f;
            Vector3 direction = Quaternion.Euler(0, angle, 0) * transform.forward;
            Vector3 targetPos = transform.position + direction * 2f;

            if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                agent.Warp(hit.position);
                break;
            }
        }

        stuckCounter = 0;
        isStuck = false;

        yield return new WaitForSeconds(0.2f);
        
        if (previousState == EnemyState.Patrolling)
        {
            SetNewPatrolPoint();
        }
        else if (agent.isOnNavMesh)
        {
            MoveToPoint(originalDestination, currentSpeed);
        }

        ChangeState(previousState);
    }

    protected virtual void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
        if (currentState == EnemyState.Attacking)
            return;

        switch (currentState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrolling:
                if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    hasSpottedPlayer = true;
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
                    hasSpottedPlayer = false;
                    ChangeState(EnemyState.Patrolling);
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

    #endregion

    #region Animation System

    protected virtual void UpdateAnimation()
    {
        if (animator == null) return;

        // Handle attack animation separately
        if (currentState == EnemyState.Attacking)
        {
            if (!isPerformingAttack)
            {
                StartAttackAnimation();
            }
            else
            {
                UpdateAttackAnimation();
            }
            return;
        }

        // Reset attack state if we're not attacking
        isPerformingAttack = false;
        
        // Get the actual velocity magnitude
        float currentVelocityMagnitude = agent.isOnNavMesh ? agent.velocity.magnitude : 0f;
        bool isActuallyMoving = currentVelocityMagnitude > movementThreshold;

        // Determine the movement state
        bool isIdle = !isActuallyMoving;
        bool isWalking = isActuallyMoving && currentSpeed <= walkSpeed * 1.5f;
        bool isRunning = isActuallyMoving && currentSpeed > walkSpeed * 1.5f;

        // Update animation state with attack explicitly set to false
        UpdateAnimationState(isIdle, isWalking, isRunning, false);
    }

    protected virtual void StartAttackAnimation()
    {
        isPerformingAttack = true;
        currentAttackTime = 0f;
        UpdateAnimationState(false, false, false, true);
    }

    protected virtual void UpdateAttackAnimation()
    {
        currentAttackTime += Time.deltaTime;
        
        // Keep the attack animation playing for its full duration
        if (currentAttackTime >= attackAnimationDuration)
        {
            isPerformingAttack = false;
            // Only reset to idle if we're still in attack state
            if (currentState == EnemyState.Attacking)
            {
                UpdateAnimationState(true, false, false, false);
            }
        }
    }
    
    protected virtual void UpdateAnimationState(bool idle, bool walking, bool running, bool attacking)
    {
        if (animator == null) return;

        // Set all states to false first
        animator.SetBool("IsIdle", false);
        animator.SetBool("IsWalking", false);
        animator.SetBool("IsRunning", false);
        animator.SetBool("IsAttacking", false);

        // Then set only the active state
        if (attacking)
        {
            animator.SetBool("IsAttacking", true);
        }
        else if (running)
        {
            animator.SetBool("IsRunning", true);
        }
        else if (walking)
        {
            animator.SetBool("IsWalking", true);
        }
        else if (idle)
        {
            animator.SetBool("IsIdle", true);
        }
    }

    #endregion

    #region State Management

    protected virtual void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;
        
        // If we're leaving attack state, ensure we complete the current attack animation
        if (currentState == EnemyState.Attacking && isPerformingAttack)
        {
            if (currentAttackTime < attackAnimationDuration)
            {
                return;
            }
        }

        currentState = newState;
        
        // Reset movement and update speed based on new state
        if (agent != null && agent.isOnNavMesh)
        {
            agent.isStopped = false;
            agent.velocity = Vector3.zero;
        }

        switch (newState)
        {
            case EnemyState.Idle:
                currentSpeed = 0f;
                isMoving = false;
                break;
            case EnemyState.Patrolling:
                currentSpeed = walkSpeed;
                break;
            case EnemyState.Pursuing:
                currentSpeed = runSpeed;
                isMoving = true;
                break;
            case EnemyState.Attacking:
                StopMovement();
                break;
            case EnemyState.Fleeing:
                currentSpeed = maxSpeed;
                isMoving = true;
                break;
        }
    }

    #endregion

    #region State Handlers

    protected virtual void HandleIdle()
    {
        StopMovement();
        UpdateAnimationState(true, false, false, false);

        if (Time.time >= patrolWaitEndTime)
        {
            SetNewPatrolPoint();
            ChangeState(EnemyState.Patrolling);
        }
    }

    protected virtual void HandlePatrolling()
    {
        if (isWaitingAtPatrolPoint)
        {
            if (Time.time >= patrolWaitEndTime)
            {
                isWaitingAtPatrolPoint = false;
                SetNewPatrolPoint();
            }
            return;
        }

        if (!agent.isOnNavMesh) return;

        float distanceToPatrolPoint = Vector3.Distance(transform.position, currentPatrolPoint);
        
        if (distanceToPatrolPoint < 0.5f)
        {
            StartWaitAtPatrolPoint();
        }
        else
        {
            currentSpeed = walkSpeed;
            MoveToPoint(currentPatrolPoint, walkSpeed);
            
            if (agent.velocity.magnitude > movementThreshold)
            {
                UpdateAnimationState(false, true, false, false);
            }
        }
    }

    protected virtual void HandlePursuing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        MoveToPoint(player.position, runSpeed);
        FaceTarget(player.position);
        UpdateAnimationState(false, false, true, false);

        if (movementSounds.Length > 0 && Time.time % 3 < 0.1f)
        {
            PlayRandomSound(movementSounds);
        }
    }

    protected virtual void HandleFleeing()
    {
        if (!agent.isOnNavMesh || player == null) return;

        Vector3 fleeDirection = (transform.position - player.position).normalized;
        Vector3 fleePosition = transform.position + fleeDirection * detectionRange;
        
        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, detectionRange, NavMesh.AllAreas))
        {
            MoveToPoint(hit.position, maxSpeed);
            FaceTarget(transform.position + fleeDirection);
            UpdateAnimationState(false, false, true, false);
        }
    }

    protected abstract void HandleAttacking();

    public virtual void OnDamageReceived()
    {
        if (player != null)
        {
            ChangeState(EnemyState.Pursuing);
        }
    }

    #endregion

    #region Movement and Navigation

    protected virtual void MoveToPoint(Vector3 point, float speed)
    {
        if (!agent.isOnNavMesh) return;
        
        // Reset stuck detection when starting new movement
        stuckCounter = 0;
        isStuck = false;
        
        // Smooth acceleration/deceleration
        agent.acceleration = speed * 2;
        agent.angularSpeed = rotationSpeed * 100;
        
        agent.isStopped = false;
        agent.speed = speed;
        
        // Validate destination before setting
        if (NavMesh.SamplePosition(point, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
            isMoving = true;
        }
        else
        {
            Debug.LogWarning($"Invalid navigation point for {gameObject.name}");
            StopMovement();
        }
    }

    protected virtual void StopMovement()
    {
        if (!agent.isOnNavMesh) return;
        
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        agent.ResetPath();
        isMoving = false;
        
        // Update patrol point whenever we stop
        if (currentState == EnemyState.Patrolling || currentState == EnemyState.Idle)
        {
            SetNewPatrolPoint();
        }
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

    protected virtual void SetNewPatrolPoint()
    {
        int maxAttempts = 30;
        int attempts = 0;
        Vector3 newPoint;
        bool validPointFound = false;

        do
        {
            newPoint = transform.position + Random.insideUnitSphere * patrolRadius;
            newPoint.y = transform.position.y;
            attempts++;

            float distanceToNew = Vector3.Distance(transform.position, newPoint);
            
            // Check if point is on NavMesh and path is valid
            if (distanceToNew >= minPatrolDistance && 
                NavMesh.SamplePosition(newPoint, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    currentPatrolPoint = hit.position;
                    validPointFound = true;
                    break;
                }
            }
        } while (attempts < maxAttempts);

        if (!validPointFound)
        {
            currentPatrolPoint = transform.position;
            StartWaitAtPatrolPoint();
        }
    }

    protected virtual void EnforceEnemySeparation()
    {
        if (!agent.isOnNavMesh || !isMoving) return;

        Vector3 separationVector = GetSeparationVector();
        
        if (separationVector.magnitude > 0.1f)
        {
            Vector3 desiredDirection = (agent.desiredVelocity.normalized + separationVector.normalized).normalized;
            Vector3 targetPosition = transform.position + desiredDirection * 2f;
            
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            {
                if (currentState != EnemyState.Attacking)
                {
                    Vector3 currentDestination = agent.destination;
                    Vector3 adjustedDestination = Vector3.Lerp(currentDestination, hit.position, 0.5f);
                    
                    if (Vector3.Distance(currentDestination, adjustedDestination) > 0.5f)
                    {
                        agent.SetDestination(adjustedDestination);
                        float separationSpeedBoost = Mathf.Lerp(1f, 1.5f, separationVector.magnitude);
                        currentSpeed = Mathf.Max(currentSpeed, walkSpeed * separationSpeedBoost);
                    }
                }
            }
        }
    }

    private Vector3 GetSeparationVector()
    {
        Vector3 separationVector = Vector3.zero;
        int neighborCount = 0;
        
        Collider[] hitColliders = Physics.OverlapSphere(transform.position, separationDistance);
        foreach (var hitCollider in hitColliders)
        {
            BaseEnemy otherEnemy = hitCollider.GetComponent<BaseEnemy>();
            if (otherEnemy != null && otherEnemy != this)
            {
                Vector3 awayFromOther = transform.position - hitCollider.transform.position;
                float distance = awayFromOther.magnitude;
                
                if (distance < separationDistance)
                {
                    float separationStrength = 1.0f - (distance / separationDistance);
                    separationVector += awayFromOther.normalized * separationStrength;
                    neighborCount++;
                }
            }
        }
        
        if (neighborCount > 0)
        {
            separationVector /= neighborCount;
        }
        
        return separationVector;
    }

    #endregion

    #region Utility Functions

    protected virtual void PlayRandomSound(AudioClip[] sounds, float volumeMultiplier = 1f, bool canPlay = false)
    {
        bool checkAudioPlaying = canPlay? false : audioSource.isPlaying;


        if (sounds.Length > 0 && audioSource != null && !checkAudioPlaying)
        {
            AudioClip randomSound = sounds[Random.Range(0, sounds.Length)];
            audioSource.volume = volumeMultiplier;
            audioSource.PlayOneShot(randomSound);
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
            int layerMask = ~(LayerMask.GetMask("Enemy"));
            
            if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRange, layerMask))
            {
                return hit.transform == player;
            }
        }
        return false;
    }

    protected void FindPlayer()
    {
        if (player != null) return;
        
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogWarning($"Player not found by {gameObject.name}!");
        }
    }

    protected virtual void StartWaitAtPatrolPoint()
    {
        isWaitingAtPatrolPoint = true;
        patrolWaitEndTime = Time.time + waitTimeAtPatrolPoint;
        StopMovement();
        
        UpdateAnimationState(true, false, false, false);
        
        if (idleSounds.Length > 0)
        {
            PlayRandomSound(idleSounds);
        }
    }

    protected IEnumerator InitializePatrol()
    {
        yield return new WaitForSeconds(1f);
        if (enabled && gameObject.activeInHierarchy)
        {
            SetNewPatrolPoint();
            ChangeState(EnemyState.Idle);
            patrolWaitEndTime = Time.time + waitTimeAtPatrolPoint;
            isWaitingAtPatrolPoint = true;
            UpdateAnimationState(true, false, false, false);
        }
    }

    #endregion

    #region Debug

    protected virtual void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Attack range
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Patrol radius
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, patrolRadius);

        // Field of view
        Gizmos.color = Color.green;
        float halfFOV = fieldOfViewAngle * 0.5f;
        Vector3 rightDir = Quaternion.Euler(0, halfFOV, 0) * transform.forward;
        Vector3 leftDir = Quaternion.Euler(0, -halfFOV, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);

        // Current patrol point
        if (currentPatrolPoint != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentPatrolPoint, 0.5f);
            Gizmos.DrawLine(transform.position, currentPatrolPoint);
        }

        // Draw path if available
        if (agent != null && agent.hasPath)
        {
            Gizmos.color = Color.white;
            Vector3[] corners = agent.path.corners;
            for (int i = 0; i < corners.Length - 1; i++)
            {
                Gizmos.DrawLine(corners[i], corners[i + 1]);
            }
        }
    }

    #endregion
}