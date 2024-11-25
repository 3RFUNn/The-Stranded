using UnityEngine;
using UnityEngine.AI;
using System;

public abstract class BaseEnemy : MonoBehaviour
{
    #region Components
    protected NavMeshAgent Agent { get; private set; }
    protected Animator Animator { get; private set; }
    protected AudioSource AudioSource { get; private set; }
    protected Transform Player { get; private set; }
    #endregion

    #region Events
    public event Action<EnemyState> OnStateChanged;
    public event Action<int> OnDamageTaken;
    #endregion

    #region Serialized Fields
    [Header("Detection Settings")]
    [SerializeField] protected float detectionRange = 15f;
    [SerializeField] protected float fieldOfViewAngle = 180f;

    [Header("Movement Settings")]
    [SerializeField] protected float walkSpeed = 2f;
    [SerializeField] protected float runSpeed = 5f;
    [SerializeField] protected float rotationSpeed = 5f;

    [Header("Audio")]
    [SerializeField] protected AudioClip[] attackSounds;
    [SerializeField] protected AudioClip[] movementSounds;
    [SerializeField] protected AudioClip[] idleSounds;
    #endregion

    #region State Management
    protected EnemyState CurrentState { get; private set; }
    protected Vector3 LastKnownPlayerPosition { get; private set; }
    #endregion

    #region Unity Lifecycle
    protected virtual void Start()
    {
        InitializeComponents();
        if (!VerifyComponents()) enabled = false;
        InitializeState();
    }

    protected virtual void Update()
    {
        if (!IsPlayerValid()) return;
        UpdateState();
        HandleCurrentState();
    }

    protected virtual void OnDisable()
    {
        StopAllCoroutines();
        ResetAgent();
    }
    #endregion

    #region Initialization
    private void InitializeComponents()
    {
        Agent = GetComponent<NavMeshAgent>();
        Animator = GetComponent<Animator>();
        AudioSource = GetComponent<AudioSource>();
        FindPlayer();
    }

    private bool VerifyComponents()
    {
        if (Agent == null)
        {
            Debug.LogError($"[{gameObject.name}] NavMeshAgent required!");
            return false;
        }
        
        // Animator and AudioSource are optional but warn if missing
        if (Animator == null) Debug.LogWarning($"[{gameObject.name}] No Animator found!");
        if (AudioSource == null) Debug.LogWarning($"[{gameObject.name}] No AudioSource found!");
        
        return true;
    }

    protected virtual void InitializeState()
    {
        ChangeState(EnemyState.Idle);
    }
    #endregion

    #region State Management
    protected void ChangeState(EnemyState newState)
    {
        if (CurrentState == newState) return;
        
        EnemyState oldState = CurrentState;
        CurrentState = newState;
        
        OnStateChanged?.Invoke(newState);
        HandleStateTransition(oldState, newState);
    }

    protected virtual void HandleStateTransition(EnemyState oldState, EnemyState newState)
    {
        // Reset agent and animations on state change
        ResetAnimations();
        
        switch (newState)
        {
            case EnemyState.Idle:
                StopAgent();
                PlayAnimation("IsIdle");
                break;
            case EnemyState.Patrolling:
                SetAgentSpeed(walkSpeed);
                PlayAnimation("IsWalking");
                break;
            case EnemyState.Pursuing:
                SetAgentSpeed(runSpeed);
                PlayAnimation("IsRunning");
                break;
        }
    }

    protected abstract void UpdateState();
    protected abstract void HandleCurrentState();
    #endregion

    #region Detection
    protected bool IsPlayerInDetectionRange()
    {
        if (!IsPlayerValid()) return false;
        
        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        return distanceToPlayer <= detectionRange;
    }

    protected bool IsPlayerInFieldOfView()
    {
        if (!IsPlayerValid()) return false;

        Vector3 directionToPlayer = (Player.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        
        if (angle <= fieldOfViewAngle * 0.5f)
        {
            LastKnownPlayerPosition = Player.position;
            return true;
        }
        return false;
    }

    protected bool HasLineOfSightToPlayer()
    {
        if (!IsPlayerValid()) return false;

        Vector3 directionToPlayer = Player.position - transform.position;
        if (Physics.Raycast(transform.position, directionToPlayer, out RaycastHit hit, detectionRange))
        {
            return hit.transform == Player;
        }
        return false;
    }
    #endregion

    #region Movement
    protected void MoveToPosition(Vector3 position)
    {
        if (!Agent.isOnNavMesh) return;
        Agent.isStopped = false;
        Agent.SetDestination(position);
    }

    protected void StopAgent()
    {
        if (!Agent.isOnNavMesh) return;
        Agent.isStopped = true;
        Agent.velocity = Vector3.zero;
    }

    protected void SetAgentSpeed(float speed)
    {
        if (Agent.isOnNavMesh)
        {
            Agent.speed = speed;
        }
    }

    protected void FaceTarget(Vector3 target)
    {
        Vector3 direction = (target - transform.position).normalized;
        direction.y = 0;
        
        if (direction != Vector3.zero)
        {
            Quaternion lookRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * rotationSpeed);
        }
    }
    #endregion

    #region Animation
    protected void PlayAnimation(string triggerName, bool value = true)
    {
        if (Animator != null)
        {
            ResetAnimations();
            Animator.SetBool(triggerName, value);
        }
    }

    protected void ResetAnimations()
    {
        if (Animator == null) return;
        
        Animator.SetBool("IsIdle", false);
        Animator.SetBool("IsWalking", false);
        Animator.SetBool("IsRunning", false);
        Animator.SetBool("IsAttacking", false);
    }
    #endregion

    #region Audio
    protected void PlaySound(AudioClip[] clips, float volumeMultiplier = 1f)
    {
        if (AudioSource == null || clips == null || clips.Length == 0) return;
        if (AudioSource.isPlaying) return;

        AudioClip randomClip = clips[UnityEngine.Random.Range(0, clips.Length)];
        AudioSource.volume = volumeMultiplier;
        AudioSource.PlayOneShot(randomClip);
    }
    #endregion

    #region Utility
    private void FindPlayer()
    {
        Player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (Player == null)
        {
            Debug.LogWarning($"[{gameObject.name}] Player not found!");
        }
    }

    protected bool IsPlayerValid()
    {
        if (Player == null)
        {
            FindPlayer();
            return Player != null;
        }
        return true;
    }

    private void ResetAgent()
    {
        if (Agent != null && Agent.isOnNavMesh)
        {
            Agent.isStopped = true;
            Agent.velocity = Vector3.zero;
        }
    }
    #endregion

    #region Debug
    protected virtual void OnDrawGizmosSelected()
    {
        // Detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        // Field of view
        Gizmos.color = Color.green;
        float halfFOV = fieldOfViewAngle * 0.5f;
        Vector3 rightDir = Quaternion.Euler(0, halfFOV, 0) * transform.forward;
        Vector3 leftDir = Quaternion.Euler(0, -halfFOV, 0) * transform.forward;
        Gizmos.DrawRay(transform.position, rightDir * detectionRange);
        Gizmos.DrawRay(transform.position, leftDir * detectionRange);
    }
    #endregion
}