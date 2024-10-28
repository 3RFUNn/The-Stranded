using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienMove : MonoBehaviour
{
    public WhacAnAlien game;
    public PillarMove Pillar;
    public bool isHide;
    public Transform Top;
    public Transform Bottom;
    public bool shouldMoveUp;
    public bool shouldMoveDown;
    public float speed;
    public float t;
    void Start()
    {
        if (isHide) {
            t = 0;
            transform.position = Bottom.position;
            Pillar.transform.position = Pillar.Bottom.position;
            
        }
        else {
            t = 1;
            transform.position = Top.position;
            Pillar.transform.position = Pillar.Top.position;
        }
        shouldMoveDown = false;
        shouldMoveUp = false;
    }

    void Update()
    {
        if (shouldMoveUp) {
            t += Time.deltaTime;
            if(t > 1){
                t = 1;
                shouldMoveUp = false;
            }
            transform.position = Vector3.Lerp(Bottom.position, Top.position, t * speed);
            Pillar.transform.position = Vector3.Lerp(Pillar.Bottom.position, Pillar.Top.position, t * speed);
        }
        else if (shouldMoveDown){
            t -= Time.deltaTime;
            if (t < 0) {
                t = 0;
                game.StartCountingDown();
                shouldMoveDown = false;
            }
            transform.position = Vector3.Lerp(Bottom.position, Top.position, t * speed);
            Pillar.transform.position = Vector3.Lerp(Pillar.Bottom.position, Pillar.Top.position, t * speed);
        }
        
    }

    public void OnClick(){
        if (shouldMoveUp) {
            shouldMoveUp = false;
        }
        shouldMoveDown = true;
    }

    public void Move(){
        
    }
}
