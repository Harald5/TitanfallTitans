using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class EnemyMovement : MonoBehaviour
{
    bool isMoving;
    public bool isDead;
    public bool isAttacking;
    public bool getHit;
    public float attackTimer;

    public NavMeshAgent agent;
    public Transform player;
    public EnterTitan et;

    public int Health;

    public Animator roninAnims;
    public AudioSource roninSource;
    public AudioClip run;
    public AudioClip swing;
    bool alreadyPlaying;
    public bool canHit;
    bool alreadyAttacking;
    public BoxCollider sword;

    // Update is called once per frame
    void Update()
    {
        if (et.inTitan)
        {
            agent.SetDestination(player.position);
            if (!isDead)
            {
                attackTimer -= 1f * Time.deltaTime;
                if (attackTimer <= 0.8 && isAttacking && !roninSource.isPlaying)
                {
                    canHit = true;
                    roninSource.PlayOneShot(swing);
                }
                if (attackTimer <= 0 && (isAttacking || getHit))
                {   
                    agent.isStopped = false;
                    isAttacking = false;
                    getHit = false;
                    canHit = false;
                    alreadyAttacking = false;
                    sword.enabled = false;
                }
                HandleAnimation();

                if (isMoving && !roninSource.isPlaying && !isAttacking && !getHit)
                {
                    roninSource.PlayOneShot(run); 
                }
            }
        }
    }

    void Start()
    {
        isMoving = true;
        sword.enabled = false;
    }

    void HandleAnimation()
    {
        roninAnims.SetBool("isMoving", isMoving);
    }

    void OnTriggerEnter()
    {
        Attack();
    }

    void OnTriggerStay()
    {
        Attack();
    }

    void Attack()
    {   
        if (!alreadyAttacking)
        {
            roninAnims.SetTrigger("attack");
            agent.isStopped = true;
            attackTimer = 1.3f;
            isAttacking = true;
            roninSource.Stop();
            alreadyAttacking = true;
            sword.enabled = true;
        }
    }
}
