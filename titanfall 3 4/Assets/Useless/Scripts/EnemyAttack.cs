using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyAttack : MonoBehaviour
{
    public EnemyMovement em;
    void OnTriggerEnter(Collider player)
    {
        if (em.canHit)
        {
            if (player.gameObject.CompareTag("Titan"))
            {
                player.GetComponent<TitanMovement>().TakeDamage(50);
            }
        }
    }
}
