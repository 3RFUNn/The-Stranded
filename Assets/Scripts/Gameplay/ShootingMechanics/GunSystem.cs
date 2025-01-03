using UnityEngine;
using TMPro;
using DG.Tweening;
using Cinemachine;
using Ink.Parsed;

public class GunSystem : MonoBehaviour
{
    // Gun stats
    public int damage = 10;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;

    // bools 
    bool shooting, readyToShoot, reloading;

    // References
    public Camera fpsCam; // Camera reference
    public Transform attackPoint;
    public Transform gunTransform; // Reference to the gun model's Transform
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;

    // Cinemachine
    public CinemachineImpulseSource impulseSource; // Cinemachine Impulse Source

    // Graphics
    public GameObject bulletHoleGraphicSand, bulletHoleGraphicBlood, bulletHoleGraphicMetal;
    public ParticleSystem muzzleFlash;
    public TextMeshProUGUI text;

    //kill counter 
    public TextMeshProUGUI killCounterText;

    // Recoil and animation settings
    public float recoilDistance = 0.1f;
    public float recoilDuration = 0.1f;
    public float reloadTiltAngle = 20f;
    public float reloadAnimationDuration = 0.5f;

    // Sounds
    public AudioSource audioSource;
    public AudioClip gunShotSound;

    public AudioClip emptyBulletsGunShotSound;
    public AudioClip reloadSound;

    //variable to track the kill counter
    private int killCounter;


    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }

    private void Update()
    {
        // Update text
        if (text != null)
        {
            if(bulletsLeft <= 5){
                text.SetText($"<color=red>{bulletsLeft}</color>/{magazineSize}");
            }else{
                text.SetText(bulletsLeft + "/" + magazineSize);
            }
        }
        else
        {
            Debug.LogWarning("Text is null");
        }
    }

    public void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);

        // Reload input
        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading)
        {
            Reload();
        }

        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        } else if(bulletsLeft <= 0 && !reloading){
            if (audioSource != null && emptyBulletsGunShotSound != null)
            {
                audioSource.PlayOneShot(emptyBulletsGunShotSound);
            }
        }
    }

    private void Shoot()
    {
        readyToShoot = false;

        // Play recoil animation
        PlayRecoilAnimation();

        // Play gunshot sound
        PlayGunShotSound();

        // Trigger camera shake
        TriggerCameraShake();

        // Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);

        // Calculate Direction with Spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        // RayCast
        if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);

            if (rayHit.collider.CompareTag("Enemy"))
            {
                EnemyHealth health = rayHit.collider.GetComponent<EnemyHealth>();
                if (health != null)
                {
                    health.TakeDamage(damage, IncrementKillCounter);
                }
                else
                {
                    GameObject obj = rayHit.collider.gameObject;
                    Destroy(obj);
                    IncrementKillCounter();
                }
                GameObject bulletHole = Instantiate(bulletHoleGraphicBlood, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                bulletHole.transform.SetParent(rayHit.transform);
            }
            else
            {
                if (!rayHit.collider.CompareTag("InvisibleObject"))
                {
                    if (rayHit.collider.CompareTag("Sand"))
                    {
                        GameObject bulletHole = Instantiate(bulletHoleGraphicSand, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                        bulletHole.transform.SetParent(rayHit.transform);
                        // Instantiate(bulletHoleGraphicSand, rayHit.point, Quaternion.Euler(0, 180, 0));
                    }
                    else
                    {
                        GameObject bulletHole = Instantiate(bulletHoleGraphicMetal, rayHit.point, Quaternion.LookRotation(rayHit.normal));
                        bulletHole.transform.SetParent(rayHit.transform);
                        // Instantiate(bulletHoleGraphicMetal, rayHit.point, Quaternion.Euler(0, 180, 0));
                    }
                }
            }
        }

        // Muzzle flash
        muzzleFlash.Play();

        bulletsLeft--;
        bulletsShot--;
        Invoke("ResetShot", timeBetweenShooting);

        if (bulletsShot > 0 && bulletsLeft > 0){
            Invoke("Shoot", timeBetweenShots);
        }else if(bulletsLeft <= 0 && !reloading){
            if (audioSource != null && emptyBulletsGunShotSound != null)
            {
                audioSource.PlayOneShot(emptyBulletsGunShotSound);
            }
        }
    }

    private void PlayRecoilAnimation()
    {
        if (gunTransform != null)
        {
            gunTransform
                .DOLocalMoveZ(-recoilDistance, recoilDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    gunTransform.DOLocalMoveZ(0, recoilDuration).SetEase(Ease.InQuad);
                });
        }
    }

    private void PlayGunShotSound()
    {
        if (audioSource != null && gunShotSound != null)
        {
            audioSource.PlayOneShot(gunShotSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or GunShotSound is not assigned!");
        }
    }

    private void TriggerCameraShake()
    {
        if (impulseSource != null)
        {
            impulseSource.GenerateImpulse();
        }
        else
        {
            Debug.LogWarning("Cinemachine Impulse Source is not assigned!");
        }
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }

    public void Reload()
    {
        if (bulletsLeft < magazineSize && !reloading)
        {
            reloading = true;

            // Play reload sound
            PlayReloadSound();

            // Play reload animation
            PlayReloadAnimation();

            Invoke("ReloadFinished", reloadTime);
        }
    }

    private void PlayReloadSound()
    {
        if (audioSource != null && reloadSound != null)
        {
            audioSource.PlayOneShot(reloadSound);
        }
        else
        {
            Debug.LogWarning("AudioSource or ReloadSound is not assigned!");
        }
    }

    private void PlayReloadAnimation()
    {
        if (gunTransform != null)
        {
            gunTransform
                .DOLocalRotate(new Vector3(-reloadTiltAngle, 0, 0), reloadAnimationDuration)
                .SetEase(Ease.OutQuad)
                .OnComplete(() =>
                {
                    gunTransform.DOLocalRotate(Vector3.zero, reloadAnimationDuration).SetEase(Ease.InQuad);
                });
        }
    }

    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }

    private void IncrementKillCounter()
    {
        killCounter++;
        UpdateKillCounterUI();
    }

    private void UpdateKillCounterUI()
    {
        if (killCounterText != null)
        {
            killCounterText.SetText(killCounter.ToString());
        }
    }
}
