using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PillarMove : MonoBehaviour
{
    public float MoveDistance, Duration;

    // use duration in this component
    public void MoveDown(){
        MoveDown(Duration);
    }

    public void MoveDown(float duration){
        transform.DOMoveY(transform.position.y - MoveDistance, duration);
    }

    public void MoveUp(float duration) {
        transform.DOMoveY(transform.position.y + MoveDistance, duration);
    }
}
