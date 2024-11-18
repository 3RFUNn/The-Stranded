using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class EnemySpawner : MonoBehaviour
{
    [System.Serializable]
    public class EnemyConfig
    {
        public GameObject prefab;
        public EnemyType type;
        public int count = 2;
    }

    public enum EnemyType
    {
        Melee,
        Ranged,
        Cowardly
    }

    [SerializeField] private List<EnemyConfig> enemyConfigs = new List<EnemyConfig>();
    [SerializeField] private NavMeshSurface navMeshSurface;
    [SerializeField] private float spawnRadius = 20f;
    
    private void Start()
    {
        if (navMeshSurface == null)
        {
            Debug.LogError("NavMeshSurface reference is missing!");
            return;
        }

        // Ensure NavMesh is built
        navMeshSurface.BuildNavMesh();
        
        // Spawn all enemies
        foreach (var config in enemyConfigs)
        {
            if (config.prefab == null)
            {
                Debug.LogError($"Enemy prefab for {config.type} is missing!");
                continue;
            }

            // Verify the prefab has the correct component
            if (!VerifyPrefabComponent(config))
            {
                Debug.LogError($"Enemy prefab for {config.type} is missing required component!");
                continue;
            }

            for (int i = 0; i < config.count; i++)
            {
                SpawnEnemy(config);
            }
        }
    }

    private bool VerifyPrefabComponent(EnemyConfig config)
    {
        switch (config.type)
        {
            case EnemyType.Melee:
                return config.prefab.GetComponent<MeleeEnemy>() != null;
            case EnemyType.Ranged:
                return config.prefab.GetComponent<RangedEnemy>() != null;
            case EnemyType.Cowardly:
                return config.prefab.GetComponent<CowardlyEnemy>() != null;
            default:
                return false;
        }
    }

    private void SpawnEnemy(EnemyConfig config)
    {
        // Check if player exists before spawning
        if (GameObject.FindGameObjectWithTag("Player") == null)
        {
            Debug.LogError("Cannot spawn enemies: Player not found in scene!");
            return;
        }
        
        Vector3 spawnPosition = GetRandomPositionOnNavMesh();
        if (spawnPosition == Vector3.zero)
        {
            Debug.LogWarning($"Could not find valid spawn position for {config.type} enemy");
            return;
        }

        // Instantiate the enemy
        GameObject enemyObject = Instantiate(config.prefab, spawnPosition, Quaternion.identity);

        // Get the specific enemy component based on type
        BaseEnemy enemy = null;
        switch (config.type)
        {
            case EnemyType.Melee:
                enemy = enemyObject.GetComponent<MeleeEnemy>();
                break;
            case EnemyType.Ranged:
                enemy = enemyObject.GetComponent<RangedEnemy>();
                break;
            case EnemyType.Cowardly:
                enemy = enemyObject.GetComponent<CowardlyEnemy>();
                break;
        }

        if (enemy == null)
        {
            Debug.LogError($"Failed to get enemy component for {config.type}");
            Destroy(enemyObject);
            return;
        }

        // Initialize NavMeshAgent
        NavMeshAgent agent = enemyObject.GetComponent<NavMeshAgent>();
        if (agent != null)
        {
            agent.Warp(spawnPosition); // This ensures proper placement on NavMesh
        }

        Debug.Log($"Successfully spawned {config.type} enemy at {spawnPosition}");
    }

    private Vector3 GetRandomPositionOnNavMesh()
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint = transform.position + Random.insideUnitSphere * spawnRadius;
            randomPoint.y = transform.position.y; // Keep the same height as spawner

            if (NavMesh.SamplePosition(randomPoint, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
            {
                // Double check it's really on the NavMesh
                if (NavMesh.FindClosestEdge(hit.position, out NavMeshHit edgeHit, NavMesh.AllAreas))
                {
                    return hit.position;
                }
            }
        }
        return Vector3.zero;
    }

    private void OnDrawGizmosSelected()
    {
        // Draw spawn radius
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}