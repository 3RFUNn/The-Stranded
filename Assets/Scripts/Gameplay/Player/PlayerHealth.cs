using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using Unity.VisualScripting;
public class PlayerHealth : MonoBehaviour
{

    public int maxHealth = 100;
    public int currentHealth;
    public HealthBar healthBar;

    public GameObject youDied;

    [SerializeField] private TextMeshProUGUI currentHealthText;

    // Start is called before the first frame update

    public int CurrentHealth => currentHealth;
    
    
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

    public void IncreaseHealth(int healthBonus)
    {
        Debug.Log("Healed!");
        currentHealth = currentHealth + healthBonus;
        healthBar.SetHealth(currentHealth, CheckAndUpdate);
    }

    void CheckAndUpdate(){
        int updatedValue = Math.Clamp(currentHealth, 0, 100);
        currentHealthText.text = updatedValue.ToString();
        if (currentHealth <= 0 ){
            youDied.SetActive(true);
            Debug.Log("You Died!");
        }
    }

}
