using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraPerspective : MonoBehaviour
{
    public Transform PlayerCamera;
    public Transform TargetTransform;
    public float Duration = 1.5f;

    private Vector3 OriginPos, OriginRot;
    
    public void MoveToPlay(){
        PlayerCamera.DOMove(TargetTransform.position, Duration);
        PlayerCamera.DORotate(TargetTransform.rotation.eulerAngles, Duration);
    }

    public void MoveBack(){
        PlayerCamera.DOMove(OriginPos, Duration);
        PlayerCamera.DORotate(OriginRot, Duration);
    }
}
