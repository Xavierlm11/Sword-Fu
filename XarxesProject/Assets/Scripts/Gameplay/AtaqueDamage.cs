using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AtaqueDamage : MonoBehaviour
{
    public int damageAmount = 20;
    private GameObject owner;

    private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null && player.gameObject != owner)
        {
            Debug.Log("Le hace daño");
            player.ReceiveDamage(damageAmount);
            //player.Die();

        }
    }

    public void SetOwner(GameObject player)
    {
        owner = player;
    }
}

