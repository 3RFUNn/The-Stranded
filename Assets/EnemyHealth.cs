using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyHealth : MonoBehaviour
{

    public int maxHealth = 100;
    private int currentHealth;
    public HealthBar healthBar;
    [SerializeField] private GameObject parent;


    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetHealth(maxHealth);
    }

    public void TakeDamage( int damage)
    {
        Debug.Log("Take Damage!");
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth, CheckAndUpdate);
    }

    void CheckAndUpdate(){
        if(currentHealth <= 0){
            Destroy(parent);
        }
    }

}
