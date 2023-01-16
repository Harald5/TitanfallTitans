using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinigunScript : MonoBehaviour
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

    public float timeBetweenShots, timeBetweenShooting;
    public float spread;

    public int bulletsPerTap, bulletsLeft;
    int bulletsShot;

    public AudioSource titanSource;
    public AudioSource source2;
    public AudioClip[] shots;

    float timer = 0.8f;
    bool shouldTime;

    TitanMovement tm;
    bool isStarting = true;
    bool alreadyEnded;

    public Animator spin;

    void Start()
    {
        muzzleFlash.Stop();
        tm = GetComponentInParent<TitanMovement>();
    }

    void HandleInput()
    {
        canShoot = Input.GetKey(KeyCode.Mouse0);
        
        if (readyToShoot && canShoot && bulletsLeft > 0 && timer <= 0)
        {
            shouldTime = false;
            bulletsShot = bulletsPerTap;
            if (!source2.isPlaying)
            {
                source2.PlayOneShot(shots[1]);
            }
            
            Shoot();
        }

        if  (Input.GetKeyUp(KeyCode.Mouse0) && !alreadyEnded)
        {
            spin.SetBool("isSpinning", false);
            source2.Stop();
            titanSource.PlayOneShot(shots[2]);
            isStarting = true;
            alreadyEnded = true;
            shouldTime = false;
            timer = 0.8f;
        }
    }

    void StartShoot()
    {
        if (canShoot && isStarting)
        {
            titanSource.Stop();
            source2.PlayOneShot(shots[0]);
            spin.SetTrigger("startspin");
            spin.SetBool("isSpinning", true);
            
            alreadyEnded = false;
            isStarting = false;
            shouldTime = true;
        }
        if (shouldTime)
            timer -= 1f * Time.deltaTime;
    }

    void Update()
    {
        HandleInput();
        StartShoot();
        if (bulletsLeft <= 0)
        {
            spin.SetBool("isSpinning", false);
            source2.Stop();
            titanSource.PlayOneShot(shots[2]);
            isStarting = true;
            alreadyEnded = true;
            shouldTime = false;
            timer = 0.8f;
        }
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

    private void ResetShot()
    {
        readyToShoot = true;
    }
}
