using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovementWithAnimation : MonoBehaviour
{
    CharacterController controller;
    public Animator PilotAnim;
    public Transform groundCheck;

    public LayerMask groundMask;
    public LayerMask wallMask;

    Vector3 move;
    Vector3 input;
    Vector3 Yvelocity;
    Vector3 forwardDirection;

    [SerializeField] float speed;

    public float runSpeed;
    public float sprintSpeed;
    public float crouchSpeed;
    public float climbSpeed;
    public float airSpeedMultiplier;

    float gravity;
    public float normalGravity;
    public float wallRunGravity;
    public float jumpHeight;

    public float slideSpeedIncrease;
    public float wallRunSpeedIncrease;
    public float slideSpeedDecrease;
    public float wallRunSpeedDecrease;

    int jumpCharges;
    float climbTimer;
    public float maxClimbTimer;

    bool isSprinting;
    bool isCrouching;
    bool isSliding;
    [SerializeField] bool isWallRunning;
    [SerializeField] bool isGrounded;
    bool isJumping;
    bool isMoving;
    bool isEmbarking;

    float startHeight;
    float crouchHeight = 0.5f;
    float slideTimer;
    float crouchFloat;
    public float maxSlideTimer;
    Vector3 crouchingCenter = new Vector3(0, 0.5f, 0);
    Vector3 standingCenter = new Vector3(0, 0, 0);

    float wallDistance = 0.7f;
    bool onLeftWall;
    bool onRightWall;
    bool hasWallRun = false;
    bool cantWallRun;
    private RaycastHit leftWallHit;
    private RaycastHit rightWallHit;
    Vector3 wallNormal;
    Vector3 lastWall;

    bool isClimbing;
    bool hasClimbed;
    bool canClimb; 
    private RaycastHit wallHit;

    public Camera playerCamera;
    float normalFov;
    public float specialFov;
    public float cameraChangeTime;
    public float wallRunTilt;
    public float tilt;

    private string currentState;

    EnterTitan et;
    public Transform embarkPos;
    public bool titanFall;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        startHeight = transform.localScale.y;
        jumpCharges = 2;
        normalFov = playerCamera.fieldOfView;
        crouchFloat = 0f;
        et = FindObjectOfType<EnterTitan>();
    }

    void IncreaseSpeed(float speedIncrease)
    {
        speed += speedIncrease;
    }

    void DecreaseSpeed(float speedDecrease)
    {
        speed -= speedDecrease * Time.deltaTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (/*!et.isEmbarking &&*/ !et.inTitan)
        {
        HandleInput();
        CheckMoving();
        CheckWallRun();
        CheckClimbing();
        if (isGrounded && !isSliding && !isEmbarking)
        {
            GroundedMovement();
        }
        else if(!isGrounded && !isWallRunning && !isClimbing)
        {
            AirMovement();
        }
        else if(isSliding)
        {
            SlideMovement();
            DecreaseSpeed(slideSpeedDecrease);
            slideTimer -= 1f * Time.deltaTime;
            if (slideTimer <= 0)
            {
                isSliding = false;
            }
        }
        else if(isWallRunning)
        {
            WallRunMovement();
            DecreaseSpeed(wallRunSpeedDecrease);

        }
        else if (isClimbing)
        {
            ClimbMovement();
            climbTimer -= 1f * Time.deltaTime;
            if (climbTimer < 0)
            {
                isClimbing = false;
                hasClimbed = true;
            }
        }
        controller.Move(move * Time.deltaTime);
        CameraEffects();
        ApplyGravity();
        }
    }

    void FixedUpdate()
    {
        CheckGround();
    }

    void CameraEffects()
    {
        float fov = isWallRunning ? specialFov : isSliding ? specialFov : normalFov;
        playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, cameraChangeTime * Time.deltaTime);

        if (isWallRunning)
        {
            if (onRightWall)
            {
                tilt = Mathf.Lerp(tilt, wallRunTilt, cameraChangeTime * Time.deltaTime);
                ChangeAnimationState("wallrunright2");
            }
            else if (onLeftWall)
            {
                tilt = Mathf.Lerp(tilt, -wallRunTilt, cameraChangeTime * Time.deltaTime);
                ChangeAnimationState("wallrunleft2");
            }
        }
        else
        {
            tilt = Mathf.Lerp(tilt, 0f, cameraChangeTime * Time.deltaTime);
        }
    }

    void HandleAnimation()
    {

        PilotAnim.SetBool("goingUp", Yvelocity.y > 0 && !isWallRunning);
        PilotAnim.SetBool("goingDown", Yvelocity.y <= 0 && !isWallRunning && !isGrounded);
        /*if (Yvelocity.y > 0 && !isWallRunning && !isGrounded)
            ChangeAnimationState("goingUp");*/

        if (Yvelocity.y <= 0 && !isWallRunning && !isGrounded)
            ChangeAnimationState("goingDown");

        PilotAnim.SetFloat("crouch", crouchFloat, 0.1f, Time.deltaTime);
    }

    void HandleInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical"));

        PilotAnim.SetFloat("X", input.x, 0.1f, Time.deltaTime);
        PilotAnim.SetFloat("Z", input.z, 0.1f, Time.deltaTime);
        if (!isWallRunning)
        {
            input = transform.TransformDirection( input );
            input = Vector3.ClampMagnitude( input, 1f );
        }

        if (Input.GetKeyDown(KeyCode.F))
        {
            Embark();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            Titanfall();
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            Crouch();      
        }
        if (Input.GetKeyUp(KeyCode.C))
        {
            ExitCrouch();
        }

        if (Input.GetKeyDown( KeyCode.LeftShift ) && isGrounded)
        {
            isSprinting = true;
        }
        if (Input.GetKeyUp( KeyCode.LeftShift ))
        {
            isSprinting = false;
        }

        if (Input.GetKeyUp(KeyCode.Space) && jumpCharges > 0)
        {
            Jump();
        }
    } 

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 0.25f, groundMask);
        if (isGrounded)
        {
            jumpCharges = 1;
            hasWallRun = false;
            hasClimbed = false;
            climbTimer = maxClimbTimer;
        }
    }

    void CheckMoving()
    {
        if (input.x != 0 || input.z != 0)
        {
            isMoving = true;
        }
        else
        {
            isMoving = false;    
        }

    }

    void CheckWallRun()
    {
        onRightWall = Physics.Raycast(transform.position, transform.right, out rightWallHit, wallDistance, wallMask);
        onLeftWall = Physics.Raycast(transform.position, -transform.right, out leftWallHit, wallDistance, wallMask);

        if ((onRightWall || onLeftWall) && !isWallRunning)
        {
            TestWallRun();
        }
        else if (!onRightWall && !onLeftWall && isWallRunning)
        {
            ExitWallRun();
        }
    }

     void CheckClimbing()
    {
        canClimb = Physics.Raycast(transform.position, transform.forward, out wallHit, 0.7f, wallMask);
        float wallAngle = Vector3.Angle(-wallHit.normal, transform.forward);
        if (wallAngle < 15 && canClimb && !hasClimbed)
        {
            isClimbing = true;
        }
        else
        {
            isClimbing = false;
        }
    }

    void GroundedMovement()
    {
        speed = isSprinting ? sprintSpeed : isCrouching ? crouchSpeed : runSpeed;
        string state = !isMoving && isCrouching ? "crouchidle_001" : !isMoving ? "idle" : isSprinting ? "running_001" : isCrouching ? "crouchWalking2" : "run";
        ChangeAnimationState(state);
        if ( input.x != 0 )
        {
            move.x += input.x * speed;
        }
        else
        {
            move.x = 0;
        }
        if ( input.z != 0 )
        {
            move.z += input.z * speed;
        }
        else
        {
            move.z = 0;
        }

        move = Vector3.ClampMagnitude( move, speed );
    }

    void AirMovement()
    {
        move.x += input.x * airSpeedMultiplier;
        move.z += input.z * airSpeedMultiplier;
        
        move = Vector3.ClampMagnitude( move, speed );
    }

    void SlideMovement()
    {
        move += forwardDirection;
        move = Vector3.ClampMagnitude( move, speed );
        ChangeAnimationState("sliding");
    }

    void WallRunMovement()
    {
        if (input.z > (forwardDirection.z - 10f) && input.z < (forwardDirection.z + 10f))
        {
            move += forwardDirection;
        }
        else if (input.z < (forwardDirection.z - 10f) && input.z > (forwardDirection.z + 10f))
        {
            move.x = 0;
            move.z = 0;
            ExitWallRun();
        }
        move.x += input.x * airSpeedMultiplier;

        if (!onRightWall && !onLeftWall && isWallRunning)
        {
            ExitWallRun();
        }

        move = Vector3.ClampMagnitude( move, speed );
    }

    void ClimbMovement()
    {
        forwardDirection = Vector3.up;
        move.x += input.x * airSpeedMultiplier;
        move.z += input.z * airSpeedMultiplier;

        Yvelocity += forwardDirection;
        speed = climbSpeed;
        
        move = Vector3.ClampMagnitude( move, speed );
        Yvelocity = Vector3.ClampMagnitude( Yvelocity, speed );
    }

    void Crouch()
    {
        //controller.height = crouchHeight;
        //controller.center = crouchingCenter;
        //transform.localScale = new Vector3(transform.localScale.x, crouchHeight, transform.localScale.z);
        if (speed > runSpeed)
        {
            isSliding = true;
            forwardDirection = transform.forward;
            IncreaseSpeed(slideSpeedIncrease);
            slideTimer = maxSlideTimer;
        }
        isCrouching = true;
        crouchFloat = 1f;
    }
 
    void ExitCrouch()
    {
        //controller.height = (startHeight * 2);
        //controller.center = standingCenter;
        //transform.localScale = new Vector3(transform.localScale.x, startHeight, transform.localScale.z);
        isCrouching = false;
        isSliding = false;
        crouchFloat = 0f;
    }

    void TestWallRun()
    {
        wallNormal = onRightWall ? rightWallHit.normal : leftWallHit.normal;
        if (hasWallRun)
        {
            float wallAngle = Vector3.Angle(wallNormal, lastWall);
            if (wallAngle > 15)
            {
                WallRun();
            }
        }
        else
        {
            hasWallRun = true;
            WallRun();
        }
    }

    void WallRun()
    {
        isWallRunning = true;
        jumpCharges = 1;
        Yvelocity = new Vector3(0f, 0f, 0f);

        forwardDirection = Vector3.Cross(wallNormal, Vector3.up);
 
        if (Vector3.Dot(forwardDirection, transform.forward) < 0)
        {
            forwardDirection = -forwardDirection;
        }
    }

    void ExitWallRun()
    {
        isWallRunning = false;
        lastWall = wallNormal;
        IncreaseSpeed(wallRunSpeedIncrease);
        ChangeAnimationState("jump32");
    }

    void Jump()
    {
        if (!isGrounded && !isWallRunning)
        {
            jumpCharges -= 1;
        }
        else if (isWallRunning)
        {
            ExitWallRun();
            ChangeAnimationState("jump32");
        } 
        else if (isGrounded)
        {
            ChangeAnimationState("jump32");
        }

        hasClimbed = false;
        climbTimer = maxClimbTimer;
        Yvelocity.y = Mathf.Sqrt( jumpHeight * -2f * normalGravity );   
    }

    void ApplyGravity()
    {
        gravity = isWallRunning ? wallRunGravity : isClimbing ? 0f : normalGravity;
        Yvelocity.y += gravity * Time.deltaTime;
        controller.Move( Yvelocity * Time.deltaTime );
    }

    void ChangeAnimationState(string NewState)
    {
        if (NewState == currentState) return;
 
        PilotAnim.Play(NewState);
 
        currentState = NewState;
    }

    void Embark()
    {
        isEmbarking = true;
        //controller.enabled = false;
        transform.position = embarkPos.position;
        //transform.rotation = embarkPos.rotation;
        ChangeAnimationState("embarkpilot");
        //controller.enabled = true;
    }

    void Titanfall()
    {
        titanFall = true;
        et.StartFall();
    }

}
