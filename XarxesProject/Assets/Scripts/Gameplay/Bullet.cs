using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float velocidad = 5.0f;
    public Vector3 direccion = Vector3.forward;
    public int damageAmount = 20;
    public GameObject fallensword;
    private GameObject owner;
    private Rigidbody rb;
    public bool isDestroying;
    public MeshRenderer blade;

    public Vector3 positionToFall;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        isDestroying = false;
    }

    void Update()
    {
        if (owner != null && owner.GetComponent<PlayerMovement>().playerCharacter!= null && owner.GetComponent<PlayerMovement>().playerCharacter.characterLink != null && owner.GetComponent<PlayerMovement>().playerCharacter.characterLink.isLocal)
        {
            //Hace que lo bullet se mueva hacia delante
            transform.Translate(direccion * velocidad * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {

        if(owner == null)
        {
            return;
        }
        else if (owner.GetComponent<PlayerMovement>().playerCharacter != null &&
            owner.GetComponent<PlayerMovement>().playerCharacter.characterLink != null &&
            !owner.GetComponent<PlayerMovement>().playerCharacter.characterLink.isLocal)
        {
            return;
        }

        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null && player.gameObject != owner)
        {
            Debug.Log("Takes Damage");
            player.ReceiveDamage(damageAmount);
            GameplayManager.Instance.UpdateGameplayEveryOne(player.playerId,true);
            // player.Die();

        }

        if(other.tag == "Player" && other.GetComponent<PlayerCharacter>().characterLink.isLocal)
        {

        }
        else
        {
            FallSword fallSword = new FallSword(transform.position);
            fallSword.player = owner.GetComponent<PlayerMovement>().playerCharacter.characterLink.playerInfo;
            fallSword.transferType = TransferType.AllExceptLocal;
            ConnectionManager.Instance.SerializeToJsonAndSend(fallSword);

            positionToFall = transform.position;

            Debug.Log("DADO");
            Destroy(gameObject);
        }

        
    }

    public void ConvertToFallen()
    {
        if(positionToFall == Vector3.zero)
        {
            positionToFall = transform.position;
        }

        owner.GetComponent<PlayerMovement>().canSyncSword = false;
        GameObject FallenSword = Instantiate(fallensword, positionToFall, Quaternion.identity);


        //if(owner.GetComponent<PlayerMovement>().fallenSword != null)
        //{
        //    Destroy(owner.GetComponent<PlayerMovement>().fallenSword);
        //    owner.GetComponent<PlayerMovement>().fallenSword = null;
        //}

        //Debug.LogError("000");
        owner.GetComponent<PlayerMovement>().fallenSword = FallenSword;

    }

    public void SetOwner(GameObject player)
    {
        owner = player;
    }

    private void OnDestroy()
    {
        ConvertToFallen();
    }
}
