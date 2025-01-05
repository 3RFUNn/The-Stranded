using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager instance;

    public GameObject inventoryUI;

    public Text healthText;

    private int bullets = 0;
    private int health = 0;
    private bool isInventoryOpen = false;
    public bool isMainMenu = true;

    public GunSystem bullet;
    public PlayerHealth _playerHealth;
    
    public GamePhaseManager _gamePhaseManager;

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
        if (Keyboard.current.iKey.wasPressedThisFrame && !isMainMenu)
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
            bullets--;

            bullet.AddAmmo(20);

            UpdateUI();
        }
    }

    public void UseHealth()
    {
        if (health > 0 && _playerHealth.CurrentHealth < 100)
        {
            health--;

            _playerHealth.IncreaseHealth(10);

            UpdateUI();
        }
    }

    private void UpdateUI()
    {
        healthText.text = health.ToString();
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