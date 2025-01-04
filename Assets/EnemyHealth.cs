using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using System;

public class EnemyHealth : MonoBehaviour
{

    public int maxHealth = 100;
    private int currentHealth;
    public HealthBar healthBar;
    public GameObject healthBoostPrefab;
    public float spawnDelay = 3f;
    [SerializeField] private GameObject parent;
    private EnemyLoot loot;
    
    public GameObject resourcePrefab;
    public GameObject healthTokenPrefab;
    public Animator animator;
    [SerializeField] private MonoBehaviour script;
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Rigidbody rb;

    public string EnemyType = "Melee";


    // Start is called before the first frame update
    void Start()
    {
        currentHealth = maxHealth;
        healthBar.SetHealth(maxHealth);
    }

    public void TakeDamage(int damage, Action callback)
    {
        Debug.Log("Take Damage!");
        currentHealth -= damage;
        healthBar.SetHealth(currentHealth,()=>{CheckAndUpdate(callback);});
    }

    void CheckAndUpdate(Action callback = null){
        if(currentHealth <= 0){
            callback?.Invoke();
            TriggerAnimationAndDropLoot();
            StartCoroutine(SpawnHealthBoostWithDelay());
            //Instantiate(healthBoostPrefab, transform.position, Quaternion.identity);
        }
    }

    IEnumerator SpawnHealthBoostWithDelay()
    {
        yield return new WaitForSeconds(spawnDelay);
        Instantiate(healthBoostPrefab, transform.position, Quaternion.identity);
    }
    
    
    public void DropLoot()
    {
        GameObject itemToDrop = null;

        switch (EnemyType)
        {
            case "Melee":
                itemToDrop = resourcePrefab;
                break;
            case "Coward":
                itemToDrop = healthTokenPrefab;
                break;
        }

        if (itemToDrop != null)
        {
            Instantiate(itemToDrop, transform.position, Quaternion.identity);
        }

        Destroy(parent); // Destroy the enemy game object
    }

    public void TriggerAnimationAndDropLoot()
    {
        StartCoroutine(TriggerAnimationAndDropLootCoroutine());
    }

    private IEnumerator TriggerAnimationAndDropLootCoroutine()
    {
        GamePhaseManager manager = GamePhaseManager.instance;
        manager.BackgroundAudioSource.PlayOneShot(manager.EnemyDieClip);
        animator.SetBool("IsDead", true);
        FreezeAndDecreaseHeight();
        script.enabled = false;
        agent.enabled = false;
        yield return new WaitForSeconds(3f);
        DropLoot();
    }
    
    public void FreezeAndDecreaseHeight()
    {
        // Freeze position and rotation
        rb.constraints = RigidbodyConstraints.FreezePosition | RigidbodyConstraints.FreezeRotation;

        // Decrease height by 5 units
        Vector3 newPosition = parent.transform.position;
        newPosition.y -= 1;
        parent.transform.position = newPosition;
    }

}
