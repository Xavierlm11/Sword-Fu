using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 40f;


    public GameObject balaPrefab;
    public Transform puntoDeDisparo;
    public float velocidadBala = 10f;
    public float shootRate = 1f;
    float nextFireRate;

    void Update()
    {
        
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontal,0,vertical);
        movementDirection.Normalize();

        transform.position = transform.position + movementDirection * speed * Time.deltaTime;

        if(movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementDirection), rotationSpeed * Time.deltaTime);
        }


        if (Input.GetButtonDown("Fire1"))
        {
            
                Disparar();
            
        }
        
    }

    void Disparar()
    {
        if (Time.time > nextFireRate)
        {
            GameObject bala = Instantiate(balaPrefab, puntoDeDisparo.position, puntoDeDisparo.rotation);
            Rigidbody rbBala = bala.GetComponent<Rigidbody>();

            nextFireRate = Time.time + shootRate;


            if (rbBala != null)
            {
                rbBala.velocity = bala.transform.forward * velocidadBala;
            }


            Destroy(bala, 5f);

        }
    }

}
