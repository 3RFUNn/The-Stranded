using UnityEngine;
using TMPro;
//using EZCameraShake;


public class GunSystem : MonoBehaviour
{
    //Gun stats
    public int damage = 10;
    public float timeBetweenShooting, spread, range, reloadTime, timeBetweenShots;
    public int magazineSize, bulletsPerTap;
    public bool allowButtonHold;
    int bulletsLeft, bulletsShot;


    //bools 
    bool shooting, readyToShoot, reloading;


    //Reference
    public Camera fpsCam;
    public Transform attackPoint;
    public RaycastHit rayHit;
    public LayerMask whatIsEnemy;


    //Graphics
    public GameObject bulletHoleGraphicSand, bulletHoleGraphicBlood, bulletHoleGraphicMetal;

    public ParticleSystem muzzleFlash;
    //public CameraShaker camShake; // To be done later
    public float camShakeMagnitude, camShakeDuration;
    public TextMeshProUGUI text;


    private void Awake()
    {
        bulletsLeft = magazineSize;
        readyToShoot = true;
    }
    private void Update()
    {
        //MyInput();


        //SetText
        if(text != null){
            text.SetText(bulletsLeft + "/" + magazineSize);
        }else{
            Debug.LogWarning("Text is null");
        }
    }
    public void MyInput()
    {
        if (allowButtonHold) shooting = Input.GetKey(KeyCode.Mouse0);
        else shooting = Input.GetKeyDown(KeyCode.Mouse0);


        //if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < magazineSize && !reloading) Reload();


        //Shoot
        if (readyToShoot && shooting && !reloading && bulletsLeft > 0)
        {
            bulletsShot = bulletsPerTap;
            Shoot();
        }
    }
    private void Shoot()
    {
        readyToShoot = false;


        //Spread
        float x = Random.Range(-spread, spread);
        float y = Random.Range(-spread, spread);


        //Calculate Direction with Spread
        Vector3 direction = fpsCam.transform.forward + new Vector3(x, y, 0);

        bool bulletHoleInstantiated = false;
        //RayCast
        if (Physics.Raycast(fpsCam.transform.position, direction, out rayHit, range, whatIsEnemy))
        {
            Debug.Log(rayHit.collider.name);


            if (rayHit.collider.CompareTag("Enemy"))
            {
                EnemyHealth health = rayHit.collider.GetComponent<EnemyHealth>();
                if(health != null){
                    health.TakeDamage(damage);
                }else{
                    GameObject obj = rayHit.collider.gameObject;
                    Destroy(obj);
                }
                GameObject bulletHole = Instantiate(bulletHoleGraphicBlood, rayHit.point, Quaternion.LookRotation(rayHit.normal));

                // Set the bullet hole as a child of the object it hits to ensure it moves with the object
                bulletHole.transform.SetParent(rayHit.transform);
            }else{
                if (!rayHit.collider.CompareTag("InvisibleObject")){
                    if(rayHit.collider.CompareTag("Sand")){
                        Instantiate(bulletHoleGraphicSand, rayHit.point, Quaternion.Euler(0, 180, 0));
                    }else{
                        Instantiate(bulletHoleGraphicMetal, rayHit.point, Quaternion.Euler(0, 180, 0));
                    }
                }
            }
            bulletHoleInstantiated = true;
        }


        //ShakeCamera
        //camShake.Shake(camShakeDuration, camShakeMagnitude); // To be done later


        //Graphics
        // GameObject bulletEffect = rayHit.collider.CompareTag("Enemy")?bulletHoleGraphicBlood : bulletHoleGraphic;
        if(!bulletHoleInstantiated){
            Instantiate(bulletHoleGraphicSand, rayHit.point, Quaternion.Euler(0, 180, 0));
        }
        //  if (!rayHit.collider.CompareTag("InvisibleObject")){
        //     GameObject bulletHole = Instantiate(bulletEffect, rayHit.point, Quaternion.LookRotation(rayHit.normal));

        //     // Set the bullet hole as a child of the object it hits to ensure it moves with the object
        //     bulletHole.transform.SetParent(rayHit.transform);
        // }


        // GameObject bulletHole = Instantiate(bulletEffect, rayHit.point, Quaternion.LookRotation(rayHit.normal));

        // // Set the bullet hole as a child of the object it hits to ensure it moves with the object
        // bulletHole.transform.SetParent(rayHit.transform);
        muzzleFlash.Play();
        bulletsLeft--;
        bulletsShot--;
        Invoke("ResetShot", timeBetweenShooting);
        if (bulletsShot > 0 && bulletsLeft > 0)
            Invoke("Shoot", timeBetweenShots);
    }
    private void ResetShot()
    {
        readyToShoot = true;
    }
    public void Reload()
    {

        if(bulletsLeft < magazineSize && !reloading){
            reloading = true;
            Invoke("ReloadFinished", reloadTime);
        }
    }
    private void ReloadFinished()
    {
        bulletsLeft = magazineSize;
        reloading = false;
    }
}
