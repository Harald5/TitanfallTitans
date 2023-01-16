using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Animations.Rigging;

public class TitanMovement : MonoBehaviour
{
    CharacterController controller;
    public Animator titanAnims;
    public AudioSource titanSource;
    public AudioClip[] footstep;
    public AudioClip walking;
    public AudioClip running;
    public AudioClip jump;
    public AudioClip step1;
    public AudioClip step2;

    public LayerMask groundMask;
    public Transform groundCheck;
    
    float dashTimer;

    float speed;
    public float walkSpeed;
    public float runSpeed;
    public float gravity;
    public float jumpGravity;

    Vector3 input;
    Vector3 move;
    Vector3 Yvelocity;
    Vector3 forwardDirection;

    public bool isMoving;
    public bool isRunning;
    public bool isDead;
    bool isDashing;
    public bool isGrounded;
    bool isJumping;
    bool alreadyPlayingSound;
    EnterTitan et;

    float FootTimer;
    float footTimer2;
    public float maxWalkFootTimer;
    public float maxRunFootTimer;
    bool rightStep = true;
    int step;
    public cameraShake cs;

    public int Health;
    public GameObject hitScreen;
    public Animator arms;
    public Camera cam;
    public GameObject cameraOwner;
    public GameObject rifle;

    public ParticleSystem leftFlame;
    public ParticleSystem rightFlame;


    void Start()
    {
        controller = GetComponent<CharacterController>();
        et = GetComponent<EnterTitan>();
        FootTimer = maxWalkFootTimer;
        step = 0;
        leftFlame.Stop();
        rightFlame.Stop();
        //arms.SetTrigger("minigun")
    }

    void HandleInput()
    {
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0f, Input.GetAxisRaw("Vertical")); 
        
        titanAnims.SetFloat("moveX", input.x, 0.1f, Time.deltaTime);
        titanAnims.SetFloat("moveZ", input.z, 0.1f, Time.deltaTime);

        input = transform.TransformDirection( input );
        input = Vector3.ClampMagnitude( input, 1f ); 

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            isRunning = true;
            FootTimer = maxRunFootTimer;
        }
        if (Input.GetKeyUp(KeyCode.LeftShift))
        {
            isRunning = false;
            FootTimer = maxWalkFootTimer;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            BeginSlide();
        }
    }
    void Update()
    {
        if (et.inTitan && !isDead)
        {
            HandleInput();
            if (isDashing && isGrounded)
            {
                Dash();
                dashTimer -= 1f * Time.deltaTime;
                if (dashTimer <= 0)
                {
                    isDashing = false;
                }
            }
            else if (!isDashing && isGrounded)
            {
                Movement();
            }
            else if (!isGrounded)
            {
                AirMovement();
            }
            controller.Move(move * Time.deltaTime);
            HandleAnimation();
            ApplyGravity(); 
            CheckGround();
            if (!isJumping)
            {
                if (isMoving && !isRunning)
                {
                    if (rightStep)
                        FootTimer -= 1f * Time.deltaTime;
                        if (FootTimer <= 0)
                        {
                            titanSource.PlayOneShot(step1);
                            FootTimer = maxWalkFootTimer;
                            rightStep = false;
                        } 

                    if (!rightStep)
                        footTimer2 -= 1f * Time.deltaTime;
                        if (footTimer2 <= 0)
                        {
                            titanSource.PlayOneShot(step2);
                            footTimer2 = maxWalkFootTimer;
                            rightStep = true;
                        } 
                    
                }
                else if (isMoving && isRunning)
                {
                    FootTimer -= 1f * Time.deltaTime;
                    if (FootTimer <= 0)
                    {
                        titanSource.PlayOneShot(footstep[step]);
                        FootTimer = maxRunFootTimer;
                        step += 1;
                        if (step > footstep.Length - 1)
                        {
                            step = 0;
                        }
                    }      
                }
            }
        }  

        if (hitScreen.GetComponent<Image>().color.a > 0)
        {
            var color = hitScreen.GetComponent<Image>().color;
            color.a -= .5f * Time.deltaTime;
            hitScreen.GetComponent<Image>().color = color;
        }
    }

    void Movement()
    {
        speed = isRunning ? runSpeed : walkSpeed;
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

        if (input == new Vector3(0f, 0f, 0f))
        {
            isMoving = false;
        }
        else
        {
            isMoving = true;
        }

        move = Vector3.ClampMagnitude( move, speed );
    }

    void AirMovement()
    {
        move.x += input.x * 1f;
        move.z += input.z * 1f;
        
        move = Vector3.ClampMagnitude( move, speed );
    }

    void Dash()
    {
        move += forwardDirection;
        move = Vector3.ClampMagnitude( move, speed );
    }

    void BeginSlide()
    {
        speed += 27f;
        forwardDirection = input;
        dashTimer = 0.6f;
        isDashing = true;
    }
    
    void HandleAnimation()
    {
        titanAnims.SetBool("isMoving", isMoving);
        titanAnims.SetBool("isRunning", isRunning);
    }

    void ApplyGravity()
    {
        float g = isJumping ? jumpGravity : gravity;
        Yvelocity.y += g * Time.deltaTime;
        controller.Move( Yvelocity * Time.deltaTime );
    }

    public void TakeDamage(int damage)
    {
        var color = hitScreen.GetComponent<Image>().color;
        color.a = 0.5f;
        hitScreen.GetComponent<Image>().color = color;
        cs.Shake(0.15f, 0.4f);
        Health -= damage;
        if (Health <= 0)
        {
            isDead = true;
            cam.transform.parent = cameraOwner.transform;
            arms.SetBool("die", true);
            titanAnims.SetBool("die", true);
            //rifle.transform.parent = cameraOwner.transform;
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, 2f, groundMask);
        if (isGrounded && Yvelocity.y < 0f)
        {
            isJumping = false;
            leftFlame.Stop();
            rightFlame.Stop();
        }
    }

    void Jump()
    {
        titanAnims.SetTrigger("jump");
        Yvelocity.y = Mathf.Sqrt( 3f * -2f * gravity );  
        controller.Move( Yvelocity * Time.deltaTime );
        rightFlame.Play();
        leftFlame.Play();
        titanSource.PlayOneShot(jump);
        isJumping = true;
    }

}
