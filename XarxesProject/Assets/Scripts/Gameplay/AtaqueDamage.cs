using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttackDamage : MonoBehaviour
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
            Debug.Log("ESTA MUERTOOOOO " + player.isAlive);
            GameplayManager.Instance.UpdateGameplayEveryOne(player.playerId, true,false,player.playerCharacter.characterLink.playerInfo.client.nickname);
            //player.Die();

        }
    }

    public void SetOwner(GameObject player)
    {
        owner = player;
    }
}

