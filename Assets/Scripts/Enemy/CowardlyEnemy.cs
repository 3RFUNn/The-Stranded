using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class CowardlyEnemy : BaseEnemy
{
    #region Serialized Fields
    [Header("Fleeing Settings")]
    [SerializeField] private float panicDistance = 8f;
    [SerializeField] private float fleeDistance = 15f;
    [SerializeField] private float fleeSpeed = 8f;
    [SerializeField] private float lookBackInterval = 0.5f;
    
    [Header("Recovery Settings")]
    [SerializeField] private float calmDownTime = 5f;
    [SerializeField] private float hidingSpotSearchRadius = 15f;
    [SerializeField] private LayerMask hidingSpotLayers;
    [SerializeField] private float minHidingDistance = 5f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip[] panicSounds;
    #endregion

    #region Private Fields
    private float timeStartedFleeing;
    private bool isPanicked;
    private Vector3 currentHidingSpot;
    private float lastLookBackTime;
    private Coroutine hidingCoroutine;
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
                if (canSeePlayer && distanceToPlayer <= panicDistance)
                {
                    EnterPanicState();
                }
                break;

            case EnemyState.Fleeing:
                if (!isPanicked && (!canSeePlayer || distanceToPlayer > fleeDistance))
                {
                    if (Time.time - timeStartedFleeing >= calmDownTime)
                    {
                        ChangeState(EnemyState.Patrolling);
                    }
                }
                else if (canSeePlayer && distanceToPlayer <= panicDistance)
                {
                    RefreshPanicState();
                }
                break;
        }

        // Periodic look back when fleeing
        if (CurrentState == EnemyState.Fleeing && Time.time > lastLookBackTime + lookBackInterval)
        {
            LookBackAtPlayer();
        }
    }

    protected override void HandleCurrentState()
    {
        switch (CurrentState)
        {
            case EnemyState.Fleeing:
                HandleFleeing();
                break;
            case EnemyState.Patrolling:
                HandlePatrolling();
                break;
            case EnemyState.Idle:
                HandleIdle();
                break;
        }
    }

    protected override void HandleStateTransition(EnemyState oldState, EnemyState newState)
    {
        base.HandleStateTransition(oldState, newState);

        if (newState == EnemyState.Fleeing)
        {
            SetAgentSpeed(fleeSpeed);
            PlayAnimation("IsRunning");
        }
    }
    #endregion

    #region Fleeing Behavior
    private void HandleFleeing()
    {
        if (!IsPlayerValid()) return;

        // If we have a hiding spot, move towards it
        if (currentHidingSpot != Vector3.zero)
        {
            MoveToPosition(currentHidingSpot);
            
            // If we've reached the hiding spot, find a new one
            if (Vector3.Distance(transform.position, currentHidingSpot) < 0.5f)
            {
                FindNewHidingSpot();
            }
        }
        else
        {
            // Direct fleeing away from player
            Vector3 fleeDirection = GetFleeDirection();
            Vector3 targetPosition = transform.position + fleeDirection * fleeDistance;
            
            if (NavMesh.SamplePosition(targetPosition, out NavMeshHit hit, fleeDistance, NavMesh.AllAreas))
            {
                MoveToPosition(hit.position);
            }
            
            // Attempt to find a hiding spot while fleeing
            FindNewHidingSpot();
        }

        // Face away from player while fleeing
        FaceAwayFromPlayer();
    }

    private void HandleIdle()
    {
        StopAgent();
        PlayAnimation("IsIdle");
        
        // Occasionally look around nervously
        if (Random.value < 0.05f)
        {
            StartCoroutine(NervousLookAround());
        }
    }

    private void HandlePatrolling()
    {
        // More cautious patrolling
        if (Agent.remainingDistance < 0.1f)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * 5f; // Shorter patrol distance
            MoveToPosition(randomPoint);
        }
        PlayAnimation("IsWalking");
    }
    #endregion

    #region Panic State
    private void EnterPanicState()
    {
        isPanicked = true;
        timeStartedFleeing = Time.time;
        PlaySound(panicSounds);
        ChangeState(EnemyState.Fleeing);
        FindNewHidingSpot();
    }

    private void RefreshPanicState()
    {
        timeStartedFleeing = Time.time;
        if (!isPanicked)
        {
            PlaySound(panicSounds);
            isPanicked = true;
        }
    }
    #endregion

    #region Hiding Behavior
    private void FindNewHidingSpot()
    {
        if (hidingCoroutine != null)
        {
            StopCoroutine(hidingCoroutine);
        }
        hidingCoroutine = StartCoroutine(FindHidingSpotCoroutine());
    }

    private IEnumerator FindHidingSpotCoroutine()
    {
        float bestDistance = 0f;
        Vector3 bestHidingSpot = Vector3.zero;
        
        // Check multiple random positions
        for (int i = 0; i < 10; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * hidingSpotSearchRadius;
            randomPoint.y = transform.position.y;

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, hidingSpotSearchRadius, NavMesh.AllAreas))
            {
                // Check if position is behind an obstacle
                if (IsPositionHidden(hit.position))
                {
                    float distanceToPlayer = Vector3.Distance(hit.position, Player.position);
                    if (distanceToPlayer > bestDistance && distanceToPlayer > minHidingDistance)
                    {
                        bestDistance = distanceToPlayer;
                        bestHidingSpot = hit.position;
                    }
                }
            }
            
            yield return new WaitForSeconds(0.02f); // Spread the raycasts over time
        }

        if (bestHidingSpot != Vector3.zero)
        {
            currentHidingSpot = bestHidingSpot;
        }
        else
        {
            currentHidingSpot = Vector3.zero;
        }

        hidingCoroutine = null;
    }

    private bool IsPositionHidden(Vector3 position)
    {
        if (!IsPlayerValid()) return false;

        Vector3 directionToPlayer = Player.position - position;
        if (Physics.Raycast(position, directionToPlayer.normalized, out RaycastHit hit, directionToPlayer.magnitude, hidingSpotLayers))
        {
            return hit.collider.gameObject != Player.gameObject;
        }
        return false;
    }
    #endregion

    #region Movement Helpers
    private void FaceAwayFromPlayer()
    {
        if (!IsPlayerValid()) return;
        
        Vector3 awayDirection = transform.position - Player.position;
        awayDirection.y = 0;
        
        if (awayDirection != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(awayDirection);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
        }
    }

    private Vector3 GetFleeDirection()
    {
        if (!IsPlayerValid()) return transform.forward;

        Vector3 directionFromPlayer = (transform.position - Player.position).normalized;
        directionFromPlayer.y = 0;
        return directionFromPlayer;
    }

    private void LookBackAtPlayer()
    {
        lastLookBackTime = Time.time;
        if (!IsPlayerValid()) return;

        StartCoroutine(QuickLookBack());
    }

    private IEnumerator QuickLookBack()
    {
        Quaternion startRotation = transform.rotation;
        Quaternion lookBackRotation = Quaternion.LookRotation(Player.position - transform.position);
        
        float elapsed = 0f;
        float duration = 0.3f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            // Quick look back, slower return
            if (elapsed < duration * 0.4f)
            {
                transform.rotation = Quaternion.Slerp(startRotation, lookBackRotation, t * 2.5f);
            }
            else
            {
                transform.rotation = Quaternion.Slerp(lookBackRotation, startRotation, (t - 0.4f) * 1.67f);
            }
            
            yield return null;
        }
        
        transform.rotation = startRotation;
    }

    private IEnumerator NervousLookAround()
    {
        Quaternion startRotation = transform.rotation;
        float randomAngle = Random.Range(-90f, 90f);
        Quaternion targetRotation = Quaternion.Euler(0, randomAngle, 0) * startRotation;
        
        float elapsed = 0f;
        float duration = 0.5f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(startRotation, targetRotation, elapsed / duration);
            yield return null;
        }
        
        yield return new WaitForSeconds(0.2f);
        
        // Return to original rotation
        elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.rotation = Quaternion.Slerp(targetRotation, startRotation, elapsed / duration);
            yield return null;
        }
    }
    #endregion

    #region Debug
    protected override void OnDrawGizmosSelected()
    {
        base.OnDrawGizmosSelected();

        // Draw panic distance
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, panicDistance);

        // Draw flee distance
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, fleeDistance);

        // Draw current hiding spot
        if (currentHidingSpot != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(currentHidingSpot, 0.5f);
            Gizmos.DrawLine(transform.position, currentHidingSpot);
        }
    }
    #endregion
}