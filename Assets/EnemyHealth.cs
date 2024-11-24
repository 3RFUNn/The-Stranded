using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyHealth : MonoBehaviour
{
    public float health;
    public Slider slider;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        slider.value = health;
    }
    private void OnCollisionEnter(Collision obj)
    {
        if (obj.gameObject.tag == "Player")
            health = health - 10f;
    }

}
