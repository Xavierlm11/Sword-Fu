using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    // Variables
    public float speed = 5f;
    public float rotationSpeed = 40f;


    public GameObject balaPrefab;
    public Transform puntoDeDisparo;
    public float velocidadBala = 10f;
    public float shootRate = 1f;
    float nextFireRate;

    void Update()
    {
        //Detecta los imputs
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");


        //El movimiento de los jugadores hecho de forma que gire de forma suave
        Vector3 movementDirection = new Vector3(horizontal,0,vertical);
        movementDirection.Normalize();

        transform.position = transform.position + movementDirection * speed * Time.deltaTime;

        if(movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementDirection), rotationSpeed * Time.deltaTime);
        }

        //Al hacer click izquierdo del raton actiba la funcion de disparo
        if (Input.GetButtonDown("Fire1"))
        {
            
                Disparar();
            
        }
        
    }

    void Disparar()
    {
        //El if es para controlar el fire-rate
        if (Time.time > nextFireRate)
        {

            //Crea una bala en el punto de disparo

            GameObject bala = Instantiate(balaPrefab, puntoDeDisparo.position, puntoDeDisparo.rotation);
            Rigidbody rbBala = bala.GetComponent<Rigidbody>();

            nextFireRate = Time.time + shootRate;


            if (rbBala != null)
            {
                rbBala.velocity = bala.transform.forward * velocidadBala;
            }

            //Destruye la bala 5 segundos despues de ser creada
            Destroy(bala, 5f);

        }
    }

}
