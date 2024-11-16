using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySystem : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private GameObject gem;

    [SerializeField] private float maxReflectionDistance = 50f;

    private const int totalReflectionsAllowed = 4;

    // Set to store unique mirrors encountered during reflection
    private HashSet<Transform> uniqueMirrors = new HashSet<Transform>();

    private void Update() {
        lineRenderer.positionCount = 1; // Reset the line renderer count to 1 (origin point)
        lineRenderer.SetPosition(0, transform.position);
        CastRay(transform.position, transform.forward);
    }

    private void CastRay(Vector3 rayPos, Vector3 rayDir) {
        // Clear the set of unique mirrors at the start of each raycast
        uniqueMirrors.Clear();

        int reflections = 0; // Count of current reflections
        while (reflections < totalReflectionsAllowed) {
            var ray = new Ray(rayPos, rayDir);

            // Cast the ray and check if it hits anything
            if (Physics.Raycast(ray, out var rayHit, maxReflectionDistance)) {
                if (rayHit.collider.CompareTag("Mirror")) {
                    // Add the mirror to the set if not already present
                    if (uniqueMirrors.Add(rayHit.transform)) {
                        reflections++;
                        lineRenderer.positionCount++;
                        lineRenderer.SetPosition(reflections, rayHit.point);
                        Debug.DrawLine(rayPos, rayHit.point, Color.black);

                        // Update position and direction for the next ray segment
                        rayPos = rayHit.point;
                        rayDir = Vector3.Reflect(rayDir, rayHit.normal);
                    } else {
                        // If this mirror has already been reflected, stop reflecting
                        break;
                    }
                } else if (rayHit.collider.CompareTag("Gem")) {
                    // Check if all mirrors in the sequence are unique
                    // Debug.Log("")
                    // if (uniqueMirrors.Count == reflections && reflections == totalReflectionsAllowed) {
                    //     Debug.Log("Successfully Hit");
                    // }
                    Debug.Log("reflections "+ reflections);
                    Debug.Log("totalReflectionsAllowed "+ totalReflectionsAllowed);
                    if (reflections == (totalReflectionsAllowed - 1)) {
                        Debug.Log("Successfully Hit");
                    }

                    reflections++;
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(reflections, rayHit.point);
                    Debug.DrawLine(rayPos, rayHit.point, Color.green);
                    break;
                } else {
                    // If the hit object is not a mirror, stop the ray
                    reflections++;
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(reflections, rayHit.point);
                    Debug.DrawLine(rayPos, rayHit.point, Color.red);
                    break;
                }
            } else {
                // If no object was hit, stop the reflection process
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(reflections + 1, rayPos + rayDir * maxReflectionDistance);
                Debug.DrawRay(rayPos, rayDir * maxReflectionDistance, Color.red);
                break;
            }
        }
    }
}
