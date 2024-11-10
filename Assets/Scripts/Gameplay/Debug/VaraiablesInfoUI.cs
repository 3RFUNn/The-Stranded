using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VaraiablesInfoUI : MonoBehaviour
{
    public TextMeshProUGUI textUI;
    public AlienMove alienMove;
    public PillarMove pillar;
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        textUI.text = alienMove.t.ToString() + "\n";
        PrintInfo(alienMove.Pillar.transform.position, "pillar");
        PrintInfo(pillar.Top.position, "top");
        PrintInfo(pillar.Bottom.position, "bottom");
    }

    void PrintInfo(object obj, string name){
        textUI.text += name + ": " + obj.ToString() + "\n";
    }
}
