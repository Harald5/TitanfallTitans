using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class gunScript : MonoBehaviour
{
    EnterTitan et;
    public Camera cam;
    public ParticleSystem muzzleFlash;
    public GameObject impactEffect;
    public GameObject bulletHole;
    public GameObject bulletCasing;
    public Transform bulletDispenser;

    float damage = 10f;
    float range = 100f;

    bool canShoot;
    bool readyToShoot = true;
    bool isReloading = false;

    public float timeBetweenShots, timeBetweenShooting;
    public float spread;

    public int bulletsPerTap, bulletsLeft;
    int bulletsShot;

    public AudioSource titanSource;
    public AudioClip[] shots;


    TitanMovement tm;

    void Start()
    {
        muzzleFlash.Stop();
        tm = GetComponentInParent<TitanMovement>();
    }

    void HandleInput()
    {
        canShoot = Input.GetKey(KeyCode.Mouse0);
        
        if (readyToShoot && canShoot && bulletsLeft > 0 && !isReloading)
        {
            bulletsShot = bulletsPerTap;
            titanSource.Stop();
            titanSource.PlayOneShot(shots[Random.Range(0, shots.Length)]);
            Shoot();
        }

        if (Input.GetKeyDown(KeyCode.R) && bulletsLeft < 500)
        {
            StartCoroutine(Reload());
        }
    }

    void Update()
    {
        HandleInput();
    }

    private void Shoot()
    {
        readyToShoot = false;

        float xSpread = Random.Range(-spread, spread); 
        float ySpread = Random.Range(-spread, spread); 

        Vector3 direction = cam.transform.forward + new Vector3(xSpread, ySpread, 0);

        muzzleFlash.Play(); 
        if (Physics.Raycast(cam.transform.position, direction, out RaycastHit hit))
        {
            GameObject impact = Instantiate(impactEffect, hit.point, Quaternion.identity) as GameObject;
            impact.transform.forward = hit.normal;

            GameObject casing = Instantiate(bulletCasing, bulletDispenser.position, bulletDispenser.rotation) as GameObject;
            Rigidbody rb = casing.GetComponent<Rigidbody>();
            rb.AddForce(-cam.transform.right * 500f, ForceMode.Acceleration);
            
            Destroy(casing, 3f);

            if (hit.collider.CompareTag("Enemy"))
            {
                hit.collider.GetComponent<TakeDamage>().TakeDamageFunction(10);
            }
            else if (!hit.collider.CompareTag("NoBulletHole"))
            {
                GameObject hole = Instantiate(bulletHole, hit.point, Quaternion.identity) as GameObject;
                hole.transform.forward = hit.normal;

                Destroy(hole, 8f);
            }
            
        }

        bulletsLeft--;
        bulletsShot--;

        if(!IsInvoking("ResetShot") && !readyToShoot)
        {
            Invoke("ResetShot", timeBetweenShooting);
        }

        if(bulletsShot > 0 && bulletsLeft > 0)
        Invoke("Shoot", timeBetweenShots);

    }

    IEnumerator Reload()
    {
        isReloading = true;
        tm.arms.SetTrigger("reload");

        yield return new WaitForSeconds(1f);

        bulletsLeft = 500;
        isReloading = false;
    }

    private void ResetShot()
    {
        readyToShoot = true;
    }
}
