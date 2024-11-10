using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    public Slider slider;

    private float currentDisplayedHealth;
    // Start is called before the first frame update
    void Start()
    {
        currentDisplayedHealth = slider.value;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentDisplayedHealth != slider.value)
        {
            currentDisplayedHealth = Mathf.Lerp(currentDisplayedHealth, slider.value, Time.deltaTime * 10);

            slider.value = currentDisplayedHealth;
        }

    }

    public void SetMaxHealth( int health)
    {
        slider.maxValue = health;

        slider.value = health;

        currentDisplayedHealth = health;
    }


    public void SetHealth( int health)
    {
        slider.value = health;
    }
}
