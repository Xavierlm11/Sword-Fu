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

    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    void Update()
    {
        //Hace que ña bala se mueva hacia delante
        transform.Translate(direccion * velocidad * Time.deltaTime);
    }
    
        private void OnTriggerEnter(Collider other)
    {
        PlayerMovement player = other.GetComponent<PlayerMovement>();

        if (player != null && player.gameObject != owner)
        {
            Debug.Log("Le hace daño");
            player.ReceiveDamage(damageAmount);
            player.Die();

        }

        Destroy(this.gameObject);
        GameObject FallenSword = Instantiate(fallensword);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Muro"))
        {
            Debug.Log("Destroy bullet");
            Destroy(this.gameObject);
        }
    }

    public void SetOwner(GameObject player)
    {
        owner = player;
    }
}
