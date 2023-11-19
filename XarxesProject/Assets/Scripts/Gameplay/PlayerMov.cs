using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float rotationSpeed = 20f; // Cambiado a float y movido fuera de la función Update
    
    void Update()
    {
        // Obtener la entrada del jugador
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        Vector3 movementDirection = new Vector3(horizontal,0,vertical);
        movementDirection.Normalize();

        transform.position = transform.position + movementDirection * speed * Time.deltaTime;

        if(movementDirection != Vector3.zero)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementDirection), rotationSpeed * Time.deltaTime);
        }
     
    }
}
