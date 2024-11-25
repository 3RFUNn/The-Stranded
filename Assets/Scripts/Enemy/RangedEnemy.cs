using UnityEngine;
using System.Collections;

public class RangedEnemy : BaseEnemy
{
    #region Serialized Fields
    [Header("Ranged Combat Settings")]
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private Transform projectileSpawnPoint;
    [SerializeField] private float projectileSpeed = 15f;
    [SerializeField] private float projectileArcHeight = 0.1f;
    [SerializeField] private float attackDamage = 10f;
    [SerializeField] private float attackInterval = 2f;
    
    [Header("Positioning Settings")]
    [SerializeField] private float preferredDistance = 8f;
    [SerializeField] private float positioningThreshold = 2f;
    [SerializeField] private float repositionSpeed = 4f;
    
    [Header("Strafing Settings")]
    [SerializeField] private float strafeSpeed = 4f;
    [SerializeField] private float strafeChangeInterval = 2f;
    [SerializeField] private float minStrafeTime = 1f;
    [SerializeField] private float maxStrafeTime = 3f;
    #endregion

    #region Private Fields
    private float lastAttackTime;
    private float nextStrafeChangeTime;
    private bool isStrafeLeft;
    private Vector3 currentStrafeTarget;
    private bool isRepositioning;
    #endregion

    #region Unity Lifecycle
    protected override void Start()
    {
        base.Start();
        
        if (projectilePrefab == null)
        {
            Debug.LogError($"[{gameObject.name}] Projectile prefab not assigned!");
            enabled = false;
            return;
        }

        if (projectileSpawnPoint == null)
        {
            projectileSpawnPoint = transform;
            Debug.LogWarning($"[{gameObject.name}] Projectile spawn point not assigned, using transform!");
        }
    }
    #endregion

    #region State Management
    protected override void UpdateState()
    {
        if (!IsPlayerValid()) return;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        bool canSeePlayer = IsPlayerInFieldOfView() && HasLineOfSightToPlayer();

        switch (CurrentState)
        {
            case EnemyState.Idle:
            case EnemyState.Patrolling:
                if (canSeePlayer && distanceToPlayer <= detectionRange)
                {
                    ChangeState(EnemyState.Pursuing);
                }
                break;

            case EnemyState.Pursuing:
                if (IsAtPreferredRange(distanceToPlayer) && canSeePlayer)
                {
                    ChangeState(EnemyState.Attacking);
                }
                else if (!canSeePlayer && distanceToPlayer > detectionRange)
                {
                    ChangeState(EnemyState.Patrolling);
                }
                break;

            case EnemyState.Attacking:
                if (!IsAtPreferredRange(distanceToPlayer) || !canSeePlayer)
                {
                    ChangeState(EnemyState.Pursuing);
                }
                break;
        }
    }

    protected override void HandleCurrentState()
    {
        switch (CurrentState)
        {
            case EnemyState.Pursuing:
                HandlePursuing();
                break;
            case EnemyState.Attacking:
                HandleAttacking();
                break;
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;
        }
    }
    #endregion

    #region State Handlers
    private void HandlePatrolling()
    {
        // Simple patrol logic - can be expanded
        if (Agent.remainingDistance < 0.1f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 10f;
            MoveToPosition(randomPoint);
        }
        PlayAnimation("IsWalking");
    }

    private void HandlePursuing()
    {
        if (!IsPlayerValid()) return;

        float distanceToPlayer = Vector3.Distance(transform.position, Player.position);
        FaceTarget(Player.position);

        // Calculate ideal position
        Vector3 targetPosition = CalculateIdealPosition();
        MoveToPosition(targetPosition);

        // Update movement speed based on distance from ideal position
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);
        SetAgentSpeed(distanceToTarget > positioningThreshold ? runSpeed : repositionSpeed);

        // Update animation
        PlayAnimation(distanceToTarget > positioningThreshold ? "IsRunning" : "IsWalking");
    }

    private void HandleAttacking()
    {
        if (!IsPlayerValid()) return;

        FaceTarget(Player.position);
        HandleStrafing();

        if (Time.time >= lastAttackTime + attackInterval)
        {
            StartCoroutine(PerformAttackSequence());
        }
    }
    #endregion

    #region Combat
    private IEnumerator PerformAttackSequence()
    {
        lastAttackTime = Time.time;

        // Play attack animation
        PlayAnimation("IsAttacking");

        // Play attack sound
        PlaySound(attackSounds);

        // Spawn projectile
        FireProjectile();

        // Wait for animation reset
        yield return new WaitForSeconds(0.5f);

        // Reset animation if still in attack state
        if (CurrentState == EnemyState.Attacking)
        {
            PlayAnimation("IsWalking");
        }
    }

    private void FireProjectile()
    {
        if (!IsPlayerValid()) return;

        // Calculate predicted position
        Vector3 predictedPosition = PredictTargetPosition();
        
        // Spawn and setup projectile
        GameObject projectile = Instantiate(projectilePrefab, projectileSpawnPoint.position, Quaternion.identity);
        
        if (projectile.TryGetComponent<Rigidbody>(out var rb))
        {
            Vector3 directionToTarget = (predictedPosition - projectileSpawnPoint.position).normalized;
            Vector3 arcedDirection = (directionToTarget + Vector3.up * projectileArcHeight).normalized;
            rb.velocity = arcedDirection * projectileSpeed;
        }

        // Set up projectile damage (assuming projectile has a component to handle damage)
        if (projectile.TryGetComponent<IProjectile>(out var projectileComponent))
        {
            projectileComponent.Initialize(attackDamage, gameObject.layer);
        }
    }
    #endregion

    #region Movement
    private void HandleStrafing()
    {
        if (Time.time >= nextStrafeChangeTime)
        {
            isStrafeLeft = !isStrafeLeft;
            nextStrafeChangeTime = Time.time + Random.Range(minStrafeTime, maxStrafeTime);
            currentStrafeTarget = CalculateStrafePosition();
        }

        MoveToPosition(currentStrafeTarget);
        SetAgentSpeed(strafeSpeed);
    }

    private Vector3 CalculateStrafePosition()
    {
        if (!IsPlayerValid()) return transform.position;

        Vector3 strafeDirection = isStrafeLeft ? -transform.right : transform.right;
        return transform.position + strafeDirection * strafeSpeed;
    }

    private Vector3 CalculateIdealPosition()
    {
        if (!IsPlayerValid()) return transform.position;

        Vector3 directionFromPlayer = (transform.position - Player.position).normalized;
        return Player.position + directionFromPlayer * preferredDistance;
    }

    private bool IsAtPreferredRange(float distance)
    {
        return Mathf.Abs(distance - preferredDistance) <= positioningThreshold;
    }
    #endregion

    #region Targeting
    private Vector3 PredictTargetPosition()
    {
        if (!IsPlayerValid()) return Player.position;

        // If player has Rigidbody, predict movement
        if (Player.TryGetComponent<Rigidbody>(out var playerRb))
        {
            float distanceToTarget = Vector3.Distance(projectileSpawnPoint.position, Player.position);
            float timeToTarget = distanceToTarget / projectileSpeed;
            return Player.position + playerRb.velocity * timeToTarget;
        }

        return Player.position;
    }
    #endregion

    #region Debug
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw preferred range
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);

        // Draw positioning threshold
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, preferredDistance - positioningThreshold);
        Gizmos.DrawWireSphere(transform.position, preferredDistance + positioningThreshold);
    }
    #endregion
}

// Interface for projectiles
public interface IProjectile
{
    void Initialize(float damage, int ownerLayer);
}