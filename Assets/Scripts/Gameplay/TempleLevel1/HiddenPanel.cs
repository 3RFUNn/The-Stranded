using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class HiddenPanel : MonoBehaviour
{
    public PillarMove PillarMove;
    public RayGame Game;
    public CinemachineVirtualCamera VCam;
    public bool getAnswer;
    public Transform UL, UM, UR, LL, LR;


    private void Start() {
        Game.OnDestRayHitted += Puzzle3End;
        if (getAnswer) {
            UL.localEulerAngles = new Vector3(0f, 45f, 0f);
            UM.localEulerAngles = new Vector3(0f, 121.2f, 0f);
            UR.localEulerAngles = new Vector3(0f, -22.5f, 0f);
            LL.localEulerAngles = new Vector3(0f, -66.7f, 0f);
            LR.localEulerAngles = new Vector3(0f, 76.5f, 0f);
        }
    }



    async public void Puzzle3End(){
        Time.timeScale = 0f;
        await Task.Delay(500);

        VCam.Priority = 20;
        await Task.Delay(700);

        LineRenderer linerenderer = Game.lineRenderer;
        Destroy(Game);
        Destroy(linerenderer);
        PillarMove.MoveDown();
        await Task.Delay((int)PillarMove.Duration * 1000 + 700);

        VCam.Priority = 10;
        Time.timeScale = 1f;
    }
}
