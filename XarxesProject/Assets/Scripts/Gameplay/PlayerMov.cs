using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isLocal = false;
    public int playerId = 0;
    public float speed = 5f;
    public float rotationSpeed = 40f;
    private Rigidbody rb;
    public Vector3 direction;


    public GameObject balaPrefab;
    public Transform puntoDeDisparo;
    public float velocidadBala = 10f;
    public float shootRate = 1f;
    float nextFireRate;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();

        if (isLocal)
        {
            StartCoroutine(SendPlayerPositionsToServer());
        }
    }

    void Update()
    {
        //if (isLocal)
        //{


            //Detecta los imputs
            float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // El movimiento de los jugadores hecho de forma que gire de forma suave
        direction = new Vector3(horizontal, 0f, vertical).normalized;


            if (direction != Vector3.zero)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
            }

            //Al hacer click izquierdo del raton actiba la funcion de disparo
            if (Input.GetButtonDown("Fire1"))
            {

                Disparar();

            }

        //}

    }

    private void FixedUpdate()
    {
        rb.velocity = direction * speed;
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
    IEnumerator SendPlayerPositionsToServer()
    {
        while (true)
        {
            string message = "PlayerPositions,";

            Vector3 position = gameObject.transform.position;
            message += $"{gameObject.name},{position.x},{position.y},{position.z},{gameObject.transform.rotation.eulerAngles.y},{playerId}";


            ConnectionManager.Instance.Send_Data(() => ConnectionManager.Instance.SerializeToJsonAndSend(message));


        }
    }

}
