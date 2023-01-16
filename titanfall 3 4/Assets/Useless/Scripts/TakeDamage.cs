using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TakeDamage : MonoBehaviour
{
    public EnemyMovement em;
    public void TakeDamageFunction(int damage)
    {
        em.agent.isStopped = true;
        em.Health -= damage;
        if(em.Health <= 0)
        {
            em.isDead = true;
            em.getHit = false;
            em.roninAnims.SetTrigger("die");
            Destroy(this, 10f);
        }
        else
        {
            em.isAttacking = false;
            em.canHit = false;
            em.roninSource.Stop();
            em.roninAnims.SetTrigger("getHit");
            em.attackTimer = 0.7f;
            em.getHit = true;
        }
    }
}
