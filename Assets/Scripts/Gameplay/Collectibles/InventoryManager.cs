using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public GameObject inventoryUI;

    public Text healthText;

    public Text magazineCounter;

    private int bullets = 0;
    private int health = 0;
    private bool isInventoryOpen = false;
    public bool isMainMenu = true;

    public GunSystem bullet;
    public PlayerHealth _playerHealth;
    
    public GamePhaseManager _gamePhaseManager;

    public AudioSource audioSource;
    public AudioClip audioClip;

    private void Awake()
    {
        instance = this;
        _playerHealth = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerHealth>();
        bullet = _gamePhaseManager.gunSystem;
        

    }
    
    
    
    

    private void Start()
    {
        UpdateUI();
    }

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame)
        {
            ToggleInventory();
        }

        if (isInventoryOpen)
        {
            if (Keyboard.current.digit1Key.wasPressedThisFrame)
            {
                Debug.Log("bullet");
                UseBullet();
            }
            else if (Keyboard.current.digit2Key.wasPressedThisFrame)
            {
                Debug.Log("health");
                UseHealth();
            }
        }
    }

    public void ToggleInventory()
    {
        Debug.Log("Opened Inventory!");
        isInventoryOpen = !isInventoryOpen;
        inventoryUI.SetActive(isInventoryOpen);

        if (isInventoryOpen)
        {
            
            ShowAndUnLockCursor();
        }
        else
        {
            
            HideAndLockCursor();
        }
    }

    public void AddBullets()
    {
        bullets += 1;
        UpdateUI();
    }

    public void AddHealth()
    {
        health += 1;
        UpdateUI();
    }

    public void UseBullet()
    {
        if (bullets > 0)
        {
            PlaySound();
            bullets--;

            bullet.AddAmmo(20);

            UpdateUI();
        }
    }

    public void UseHealth()
    {
        if (health > 0 && _playerHealth.CurrentHealth < 100)
        {
            PlaySound();
            health--;

            _playerHealth.IncreaseHealth(10);

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        magazineCounter.text = bullets.ToString();
        healthText.text = health.ToString();
    }

    private void PlaySound(){
        if (audioSource != null && audioClip != null)
        {
            audioSource.PlayOneShot(audioClip);
        }
    }

    public void SetMainMenu(bool isMainMenu)
    {
        this.isMainMenu = isMainMenu;
    }

    public void HideAndLockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ShowAndUnLockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}