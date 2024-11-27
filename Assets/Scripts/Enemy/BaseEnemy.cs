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
    [SerializeField] protected float attackRange = 2f;
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

    [Header("Audio")] 
    [SerializeField] protected AudioClip[] attackSounds;
    [SerializeField] protected AudioClip[] movementSounds;
    [SerializeField] protected AudioClip[] idleSounds;

    protected Transform player;
    protected EnemyState currentState;
    protected bool hasSpottedPlayer = false;
    protected bool isMoving = false;

    #region Initialization

    protected virtual void Start()
    {
        InitializeComponents();
        if (VerifyComponents())
        {
            currentSpeed = 0;
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

        float distanceToPlayer = Vector3.Distance(transform.position, player.position);
        bool canSeePlayer = CheckLineOfSight();
        
        UpdateState(distanceToPlayer, canSeePlayer);
        HandleCurrentState();
        UpdateAnimation();
    }

    protected virtual void UpdateState(float distanceToPlayer, bool canSeePlayer)
    {
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

            case EnemyState.Attacking:
                if (distanceToPlayer > attackRange)
                {
                    ChangeState(EnemyState.Pursuing);
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

    protected virtual void HandleFleeing()
    {
        // Base implementation - can be overridden by derived classes
        if (!agent.isOnNavMesh || player == null) return;

        Vector3 fleeDirection = transform.position - player.position;
        Vector3 fleePosition = transform.position + fleeDirection.normalized * detectionRange;
        
        if (NavMesh.SamplePosition(fleePosition, out NavMeshHit hit, detectionRange, NavMesh.AllAreas))
        {
            MoveToPoint(hit.position, maxSpeed);
            FaceTarget(transform.position + fleeDirection);
            UpdateAnimationState(false, false, true, false);
        }
    }

    public virtual void OnDamageReceived()
    {
        if (player != null)
        {
            ChangeState(EnemyState.Pursuing);
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
            MoveToPoint(currentPatrolPoint, walkSpeed);
            UpdateAnimationState(false, true, false, false);
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

    protected abstract void HandleAttacking();

    #endregion

    #region Movement and Navigation

    protected virtual void MoveToPoint(Vector3 point, float speed)
    {
        if (!agent.isOnNavMesh) return;
        
        agent.isStopped = false;
        agent.speed = speed;
        agent.SetDestination(point);
        isMoving = true;
    }

    protected virtual void StopMovement()
    {
        if (!agent.isOnNavMesh) return;
        
        agent.isStopped = true;
        agent.velocity = Vector3.zero;
        isMoving = false;
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
            currentPatrolPoint = transform.position;
            StartWaitAtPatrolPoint();
        }
    }

    #endregion

    #region Animation and State Management

    protected virtual void UpdateAnimation()
    {
        if (animator == null) return;

        bool isIdle = !isMoving || agent.velocity.magnitude < 0.1f;
        bool isWalking = isMoving && currentSpeed <= walkSpeed * 1.5f;
        bool isRunning = isMoving && currentSpeed > walkSpeed * 1.5f;
        bool isAttacking = currentState == EnemyState.Attacking;

        UpdateAnimationState(isIdle, isWalking, isRunning, isAttacking);
    }

    protected virtual void UpdateAnimationState(bool idle, bool walking, bool running, bool attacking)
    {
        if (animator == null) return;
        
        animator.SetBool("IsIdle", idle);
        animator.SetBool("IsWalking", walking);
        animator.SetBool("IsRunning", running);
        animator.SetBool("IsAttacking", attacking);
    }

    protected virtual void ChangeState(EnemyState newState)
    {
        if (currentState == newState) return;

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
        }
    }

    #endregion

    #region Utility Functions

    protected virtual void PlayRandomSound(AudioClip[] sounds, float volumeMultiplier = 1f)
    {
        if (sounds.Length > 0 && audioSource != null && !audioSource.isPlaying)
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

        return angle <= fieldOfViewAngle * 0.5f && distanceToPlayer <= detectionRange;
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
        ChangeState(EnemyState.Idle);
        
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
    }

    #endregion
}