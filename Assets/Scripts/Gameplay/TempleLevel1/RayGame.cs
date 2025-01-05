using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using System;
using UnityEngine.Events;

public class RayGame : MonoBehaviour
{
    public Action OnDestRayHitted;

    public LineRenderer lineRenderer;
    [SerializeField] private GameObject gem;

    [SerializeField] private float maxReflectionDistance = 50f;

    public int totalReflectionsAllowed = 5;

    private HashSet<Transform> uniqueMirrors = new HashSet<Transform>();

    private Color originalGemColor;
    private Color originalLineColor;

    private void Start() {
        // Store the original color of the gem
        if (gem.TryGetComponent<Renderer>(out Renderer gemRenderer)) {
            originalGemColor = gemRenderer.material.color;
        }

        // Store the original color of the LineRenderer material
        if (lineRenderer.material != null) {
            originalLineColor = lineRenderer.material.color;
        }
    }

    private void Update() {
        lineRenderer.positionCount = 1; // Reset the line renderer count to 1 (origin point)
        lineRenderer.SetPosition(0, transform.position);

        bool gemHitSuccessfully = CastRay(transform.position, transform.forward);

        // Change the colors only on a successful hit
        if (gemHitSuccessfully) {
            ChangeGemColor(Color.green); // Change gem color to green
            ChangeLineRendererColor(Color.green); // Change LineRenderer color to gold/yellow
        } 
        //else {
        //    // Revert the colors to their original states
        //    ChangeGemColor(originalGemColor);
        //    ChangeLineRendererColor(originalLineColor);
        //}
    }

    private bool CastRay(Vector3 rayPos, Vector3 rayDir) {
        uniqueMirrors.Clear();
        int reflections = 0;
        bool gemHitSuccessfully = false;

        while (reflections < totalReflectionsAllowed) {
            var ray = new Ray(rayPos, rayDir);

            if (Physics.Raycast(ray, out var rayHit, maxReflectionDistance)) {
                if (rayHit.collider.CompareTag("Mirror")) {
                    // Add the mirror to the set if not already present
                    if (uniqueMirrors.Add(rayHit.transform)) {
                        reflections++;
                        lineRenderer.positionCount++;
                        lineRenderer.SetPosition(reflections, rayHit.point);
                        Debug.DrawLine(rayPos, rayHit.point, Color.black);

                        rayPos = rayHit.point;
                        rayDir = Vector3.Reflect(rayDir, rayHit.normal);
                    } else {
                        // If this mirror has already been reflected, stop reflecting
                        break;
                    }
                } else if (rayHit.collider.CompareTag("Gem")) {
                    // The gem is successfully hit if all mirrors are unique and total reflections match
                    if (uniqueMirrors.Count == reflections && reflections == (totalReflectionsAllowed - 1)) {
                        OnDestRayHitted?.Invoke();
                        gemHitSuccessfully = true;
                        //AppHelper.stopCrushing = true;
                        //text.color= Color.yellow;
                        //text.text = "VICTORY!";
                        //AppHelper.gameEnded = true;
                        //LevelManager.instance.EndTempleLevel();
                        //Debug.Log("Successfully Hit");
                    }

                    reflections++;
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(reflections, rayHit.point);
                    Debug.DrawLine(rayPos, rayHit.point, Color.green);
                    break;
                } else {
                    reflections++;
                    lineRenderer.positionCount++;
                    lineRenderer.SetPosition(reflections, rayHit.point);
                    Debug.DrawLine(rayPos, rayHit.point, Color.red);
                    break;
                }
            } else {
                lineRenderer.positionCount++;
                lineRenderer.SetPosition(reflections + 1, rayPos + rayDir * maxReflectionDistance);
                Debug.DrawRay(rayPos, rayDir * maxReflectionDistance, Color.red);
                break;
            }
        }

        return gemHitSuccessfully;
    }

    private void ChangeGemColor(Color color) {
        if (gem.TryGetComponent<Renderer>(out Renderer gemRenderer)) {
            gemRenderer.material.DOColor(color, 0.5f); // Smooth color transition
        }
    }

    private void ChangeLineRendererColor(Color color) {
        if (lineRenderer.material != null) {
            lineRenderer.material.DOColor(color, "_Color", 0.5f); // "_Color" is used for Albedo
        }
    }
}
