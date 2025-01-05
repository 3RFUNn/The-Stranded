using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelTrigger : MonoBehaviour
{
    [SerializeField] int level = 2;
    private void OnTriggerEnter(Collider other) {
        if(other.CompareTag("Player")){
            if(AppHelper.CurrentLevel < 2){
                AppHelper.CurrentLevel = level;
            }
        }
    }
}
