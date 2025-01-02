using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;
    public Image bar;

    public void SetHealth( int health, System.Action callback = null)
    {


        slider.value = health;
        if(health <= 30){
            bar.color = Color.red;
        }else if(health <= 70){
            Color orangeColor;
            if (ColorUtility.TryParseHtmlString("#FFA500", out orangeColor))
            {
                bar.color = orangeColor;
            }else{
                bar.color = Color.green;
            }
        }else{
            bar.color = Color.green;
        }
        callback?.Invoke();

    }
}
