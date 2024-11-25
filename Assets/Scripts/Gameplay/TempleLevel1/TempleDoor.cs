using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TempleDoor : MonoBehaviour
{
    public bool isEntrance;
    private void OnTriggerExit(Collider other) {
        if(isEntrance){
            LevelManager.instance.StartTempleLevel();
            Destroy(gameObject);
            print("Destroy");
        }else{
            LevelManager.instance.MiniMap.SetActive(false);
        }
    }
}
