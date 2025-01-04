using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    public static LevelManager instance;
    public RotatePillar TempleLevelLogic;
    public List<CrushingWall> CrushingWalls;
    public GameObject MiniMap, HintText, HintUI, Canvas;
    public Transform RightWall, RightVictory;
    public Transform Ceil, CeilVictory;
    public bool gameStarted = false;


    private void Awake() {
        instance = this;

    }

    private void Update() {
        if(gameStarted){
            if (Input.GetKeyDown(KeyCode.H)) {
                Time.timeScale = 1;
                HintUI.SetActive(!HintUI.activeSelf);
                HintText.SetActive(!HintText.activeSelf);
            }
        }
    }

    public void StartTempleLevel(){
        Canvas.SetActive(true);
        Time.timeScale = 0;
        gameStarted = true;
        MiniMap.SetActive(true);
        HintUI.SetActive(true);
        TempleLevelLogic.enabled = true;
        foreach (CrushingWall w in CrushingWalls) { 
            w.enabled = true;
        }
    }

    public void EndTempleLevel(){
        gameStarted = false;
        //MiniMap.SetActive(false);
        //HintUI.SetActive(false);
        //HintText.SetActive(false);
        Canvas.SetActive(false);
        TempleLevelLogic.enabled = false;
        foreach (CrushingWall w in CrushingWalls) {
            w.enabled = false;
        }
        RightWall.transform.DOMove(RightVictory.position, 1.5f).SetEase(Ease.OutCubic);
        Ceil.transform.DOMove(CeilVictory.position, 1.5f).SetEase(Ease.OutCubic);
    }
}
