using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlideDoor : MonoBehaviour
{
    public float MoveDistance;
    public float MoveDuration;
    public void Open() {
        transform.DOMoveX(transform.position.x + MoveDistance, MoveDuration)
            .SetUpdate(true);
    }

    public void Close() {
        transform.DOMoveX(transform.position.x - MoveDistance, MoveDuration);
    }
}
