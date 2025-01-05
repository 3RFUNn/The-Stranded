using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AlienMove : MonoBehaviour
{
    public WhacAnAlien game;
    public PillarMove Pillar;
    public Transform Top;
    public Transform Bottom;

    void Start()
    {
        
    }

    public void OnClick(){
        game.OnCountdownOver += MoveUp;
        game.OnCountdownStart?.Invoke();
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        MoveDown();
    }

    public void MoveUp(){
        transform.DOLocalMoveY(Top.localPosition.y, game.MoveDuration);
        Pillar.MoveUp(game.MoveDuration);
        game.OnCountdownOver -= MoveUp;
    }

    public void MoveDown(){
        transform.DOLocalMoveY(Bottom.localPosition.y, game.MoveDuration)
            .OnComplete(() => game.OnAlienReachedBottom());
        Pillar.MoveDown(game.MoveDuration);
    }
}
