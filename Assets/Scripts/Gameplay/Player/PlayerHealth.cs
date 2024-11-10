using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHealth : MonoBehaviour
{

    public int maxHealth = 100;
    private int currentHealth;
    public HealthBar healthBar;

    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetMaxHealth(maxHealth);
    }

    public void TakeDamage( int damage)
    {
        currentHealth += damage;
        healthBar.SetHealth(currentHealth);

        if (currentHealth <= 0 )
        {
            Die();
        }
    }

    void Die()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
