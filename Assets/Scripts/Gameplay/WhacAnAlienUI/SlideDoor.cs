using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideDoor : MonoBehaviour
{
    public float MoveDuration;
    public Transform Dest, Origin;
    //private Vector3 _origin;

    private void Start() {
    }
    public void Open() {
        transform.DOMove(Dest.position, MoveDuration)
            .SetUpdate(true);
    }

    public void Close() {
        transform.DOMove(Origin.position, MoveDuration);
    }
}
