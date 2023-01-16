using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

public class EnterTitan : MonoBehaviour
{
    bool inRange;
    public bool inTitan = false;
    bool land = true;
    bool hasLanded = true;

    public GameObject titanCamera;
    public GameObject camera2;
    public GameObject playerCamera;
    public cameraShake shaker;
    public GameObject player;
    public Animator titanAnims;
    public GameObject cameraPos;
    public GameObject cameraPos2;
    public RigBuilder builder;
    //public Rig rig;
    public GameObject bodyAim;
    public CharacterController controller;

    float timer = 2f;
    public bool isEmbarking;

    public MovementWithAnimation mva;
    Vector3 Yvelocity;
    bool isGrounded;
    bool isCloseToGround;
    public Transform groundCheck;
    public LayerMask groundMask;
    public AudioSource titanSource;
    public AudioClip falling;
    public AudioClip landing;
    public ParticleSystem fallingParticles;
    public GameObject shield;

    public GameObject rifle;
    public GameObject minigun;

    public SkinnedMeshRenderer body;
    //public MeshRenderer body2;

    TitanCamera tc;

    void Awake()
    {
        titanCamera.transform.position = cameraPos2.transform.position;
        titanCamera.transform.rotation = cameraPos2.transform.rotation;
        builder = GetComponent<RigBuilder>();
        builder.enabled = false;
        //rig.weight = 0f;
        //bodyAim.SetActive(false);
        fallingParticles.Stop();
        shield.SetActive(false);
        tc = GetComponent<TitanCamera>();
        body.enabled = false;
        //body2.enabled = false;
    }

    void OnTriggerEnter()
    {
        inRange = true;
    }

    void OnTriggerExit()
    {
        inRange = false;
    }

    void Embark()
    {
        playerCamera.SetActive(false);
        titanCamera.SetActive(true);
        mva.titanFall = false;
        titanAnims.SetTrigger("embark");
        shield.SetActive(false);
        isEmbarking = true;
    }

    void HandleInput()
    {
        if (Input.GetKeyDown(KeyCode.F) && inRange)
        {
            Embark();
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckGround();
        if (mva.titanFall)
        {
            Fall();
        }
        HandleInput();
        if (isEmbarking)
        {
            timer -= 1f * Time.deltaTime;
            if (timer <= 0)
            {
                camera2.SetActive(true);
                titanCamera.SetActive(false);
                player.SetActive(false);
                inTitan = true;
                //rig.weight = 1f;
                builder.enabled = true;
                tc.defaultY = tc.cam.transform.localPosition.y;
                isEmbarking = false;
                rifle.SetActive(true);
                minigun.SetActive(false);
            }
            /*if (timer <= 0)
            {
                //bodyAim.SetActive(true);
                isEmbarking = false;
            }*/
        }
    }

    void Fall()
    {
        Yvelocity.y += -13f * Time.deltaTime;
        controller.Move( Yvelocity * Time.deltaTime );
    }
    public void StartFall()
    {
        titanAnims.SetTrigger("Fall");
        titanSource.PlayOneShot(falling);
        fallingParticles.Play();
    }

    void Land()
    {
        if (land)
        {
            fallingParticles.Stop();
            body.enabled = true;
            //body2.enabled = false;
            titanAnims.SetBool("crouchIdle", mva.titanFall);
            titanSource.Stop();
            titanSource.PlayOneShot(landing);
            land = false;
        }
    }

    void Land2()
    {
        if (hasLanded)
        {
            StartCoroutine(shaker.Shake(1.2f, .7f));
            shield.SetActive(true);
            hasLanded = false;
        }        
    }

    void CheckGround()
    {
        isCloseToGround = Physics.Raycast(groundCheck.position, Vector3.down, 35f, groundMask);
        if (isCloseToGround)
        {
            Land();
        }
        isGrounded = Physics.CheckSphere(groundCheck.position, 1f, groundMask);
        if (isGrounded)
        {
            Land2();
        }

    }
}
