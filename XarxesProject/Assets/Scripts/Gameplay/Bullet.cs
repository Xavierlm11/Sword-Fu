using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float velocidad = 5.0f; 
    public Vector3 direccion = Vector3.forward; 

    void Update()
    {
        
        transform.Translate(direccion * velocidad * Time.deltaTime);
    }
}
