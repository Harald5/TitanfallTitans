using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterVanguardTitan : MonoBehaviour
{
    public cameraShake shaker;

    CharacterController controller;
    BoxCollider rangeCheck;

    public GameObject embarkRifle;
    public GameObject Rifle;

    public Transform groundCheck;

    public GameObject playerCamera;
    public GameObject player;
    public GameObject titanCamera;
    public GameObject embarkTitanCamera;

    public Animator titanAnimator;

    public LayerMask groundMask;

    Vector3 Yvelocity;

    bool isFalling;
    bool isGrounded;
    bool alreadyLanded;
    public bool isEmbarking;
    public bool inTitan;
    public bool inRangeForEmbark;

    public float fallingSpeed;

    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        rangeCheck = GetComponent<BoxCollider>();

        Rifle.SetActive(false);
    }

    public void StartFall()
    {
        isFalling = true;
        titanAnimator.SetTrigger("StartFall");
    }

    public IEnumerator Embark()
    {
        playerCamera.SetActive(false);
        embarkTitanCamera.SetActive(true);
        rangeCheck.enabled = false;
        titanAnimator.SetTrigger("Embark");
        isEmbarking = true;

        yield return new WaitForSeconds(1.5f);

        embarkRifle.SetActive(false);
        Rifle.SetActive(true);

        yield return new WaitForSeconds(2.4f);

        player.transform.parent = this.transform;
        player.SetActive(false);
        embarkTitanCamera.SetActive(false);
        titanCamera.SetActive(true);
        inTitan = true;
        isEmbarking = false;
    }

    void OnTriggerEnter()
    {
        inRangeForEmbark = true;
    }

    void OnTriggerExit()
    {
        inRangeForEmbark = false;
    }

    void ExitTitan()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            player.SetActive(true);
            playerCamera.SetActive(true);
            titanCamera.SetActive(false);
            inTitan = false;
            player.transform.parent = null;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (isFalling)
        {
            Fall();
        }
        if (inTitan)
        {
            ExitTitan();
        }
        if (!inTitan)
            CheckGround();
    }

    void Fall()
    {
        Yvelocity.y += fallingSpeed * Time.deltaTime;
        controller.Move( Yvelocity * Time.deltaTime );
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.4f, groundMask);
        if (isGrounded)
        {
            Land();
        }

    }

    void Land()
    {
        if (alreadyLanded == false)
            StartCoroutine(shaker.Shake(1.2f, .7f));
            alreadyLanded = true;
    }
}
