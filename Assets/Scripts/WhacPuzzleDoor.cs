using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class WhacPuzzleDoor : SlideDoor
{
    public WhacAnAlien game;

    private void Start() {
        game.OnCountdownStart += Open;
        game.OnCountdownOver += Close;
    }


    public void OnDestroy(){
       game.OnCountdownStart -= Open;
       game.OnCountdownOver -= Close;
    }
}
