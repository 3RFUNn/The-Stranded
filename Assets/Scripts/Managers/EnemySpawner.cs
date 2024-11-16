using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.AI.Navigation;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] private GameObject enemyPrefab;      // The enemy prefab to spawn
    [SerializeField] private int numberOfEnemies = 5;     // Number of enemies to spawn
    [SerializeField] private NavMeshSurface navMeshSurface; // Reference to the NavMeshSurface
    [SerializeField] private float spawnRadius = 20f;     // Radius around the spawner to generate random positions

    private List<Vector3> debugPositions = new List<Vector3>(); // Store positions for debugging

    void Start()
    {
        if (navMeshSurface != null)
        {
            SpawnEnemies();
        }
        else
        {
            Debug.LogError("NavMeshSurface reference is missing!");
        }
    }

    private void SpawnEnemies()
    {
        Debug.Log("Spawning enemies...");

        for (int i = 0; i < numberOfEnemies; i++)
        {
            Vector3 randomPosition = GetRandomPositionWithinRadius(transform.position, spawnRadius);

            if (randomPosition != Vector3.zero)
            {
                Instantiate(enemyPrefab, randomPosition, Quaternion.identity);
                debugPositions.Add(randomPosition); // Add valid positions to debug
                Debug.Log($"Enemy {i} spawned at: {randomPosition}");
            }
            else
            {
                Debug.LogWarning($"Enemy {i} could not find a valid NavMesh position.");
            }
        }
    }

    private Vector3 GetRandomPositionWithinRadius(Vector3 center, float radius)
    {
        // Generate a random position within a sphere and project it onto the NavMesh
        Vector3 randomPosition = center + Random.insideUnitSphere * radius;
        randomPosition.y = center.y; // Keep the Y coordinate consistent with the spawner's position

        Debug.Log("Generated Random Position: " + randomPosition);

        // Check if the position is valid on the NavMesh
        if (NavMesh.SamplePosition(randomPosition, out NavMeshHit hit, 10.0f, NavMesh.AllAreas))
        {
            debugPositions.Add(hit.position); // Add valid NavMesh position to debug
            Debug.Log("Valid NavMesh Position Found: " + hit.position);
            return hit.position;
        }

        Debug.LogWarning("NavMesh.SamplePosition failed at: " + randomPosition);
        return Vector3.zero; // Return zero if invalid
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        foreach (Vector3 pos in debugPositions)
        {
            Gizmos.DrawSphere(pos, 0.5f); // Draw a red sphere for each debug position
        }
    }
}
