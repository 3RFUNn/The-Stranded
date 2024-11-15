using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaySystem : MonoBehaviour
{
    [SerializeField] private LineRenderer lineRenderer;
    [SerializeField] private int numberOfRays;

    [SerializeField] private GameObject gem;

    private void Start() {
        lineRenderer.positionCount = numberOfRays + 1;
    }

    private void Update() {
        lineRenderer.SetPosition(0, transform.position);
        CastRay(transform.position, transform.forward);
    }

    private void CastRay(Vector3 rayPos, Vector3 rayDir) {
        for (int i = 0; i < numberOfRays; i++) {
            var ray = new Ray(rayPos, rayDir);
            
            // Cast the ray and check if it hits anything
            if (Physics.Raycast(ray, out var rayHit, 20)) {
                // Only reflect if the hit object has the "Mirror" tag
                if (rayHit.collider.CompareTag("Mirror")) {
                    lineRenderer.SetPosition(i + 1, rayHit.point);
                    Debug.DrawLine(rayPos, rayHit.point, Color.black);

                    // Update position and direction for the next ray segment
                    rayPos = rayHit.point;
                    rayDir = Vector3.Reflect(rayDir, rayHit.normal);
                } else if(rayHit.collider.CompareTag("Gem")){
                    gem.SetActive(false);
                }else {
                    // If it's not a mirror, end the ray at the hit point
                    lineRenderer.SetPosition(i + 1, rayHit.point);
                    Debug.DrawLine(rayPos, rayHit.point, Color.black);
                    break;
                }
            } else {
                // If no object was hit, extend the ray in the current direction
                lineRenderer.SetPosition(i + 1, rayPos + rayDir * 10f);
                Debug.DrawRay(rayPos, rayDir * 10f, Color.black);
                break;
            }
        }
    }
}
