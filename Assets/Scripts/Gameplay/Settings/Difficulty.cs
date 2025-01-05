using UnityEngine;
using System.Threading.Tasks;

public class Difficulty : MonoBehaviour
{
    public PlayerHealth[] playerHealths;
    public HealthPickup[] healthPickups;
    public GunSystem[] gunSystems;
    public EnemyHealth[] enemyHealths;
    public BaseEnemy[] enemies;

    void Awake()
    {
        playerHealths = FindObjectsOfType<PlayerHealth>();
        healthPickups = FindObjectsOfType<HealthPickup>();
        gunSystems = FindObjectsOfType<GunSystem>();
        enemyHealths = FindObjectsOfType<EnemyHealth>();
        enemies = FindObjectsOfType<BaseEnemy>();
    }

    public void easy()
    {
        PlayerPrefs.SetInt("Difficulty", 0);
        ApplyEasySettings();
    }

    public void medium()
    {
        PlayerPrefs.SetInt("Difficulty", 1);
        ApplyMediumSettings();
    }

    public void hard()
    {
        PlayerPrefs.SetInt("Difficulty", 2);
        ApplyHardSettings();
    }

    public void ApplyEasySettings()
    {
        foreach (var bonus in healthPickups)
        {
            bonus.healthBonus = 20;
        }
        foreach (var gun in gunSystems)
        {
            gun.totalAmmo = 60;
            gun.damage = 15;
        }
        foreach (var enemyHealth in enemyHealths)
        {
            enemyHealth.maxHealth = 90;
        }
        foreach (var enemy in enemies)
        {
            enemy.attackDamage1 = 8;
        }
    }

    public void ApplyMediumSettings()
    {
        // Implement medium difficulty settings here
    }

    public void ApplyHardSettings()
    {
        foreach (var bonus in healthPickups)
        {
            bonus.healthBonus = 5;
        }
        foreach (var gun in gunSystems)
        {
            gun.totalAmmo = 40;
            gun.damage = 10;
        }
        foreach (var enemyHealth in enemyHealths)
        {
            enemyHealth.maxHealth = 110;
        }
        foreach (var enemy in enemies)
        {
            enemy.attackDamage1 = 15;
        }
    }
    
    public async void ActivateAndDeactivate(GameObject obj)
    {
        obj.SetActive(true);
        await Task.Delay(1000); // 1000 milliseconds = 1 second
        obj.SetActive(false);
    }
    
    
}