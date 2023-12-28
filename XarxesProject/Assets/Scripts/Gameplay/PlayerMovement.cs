using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public bool isLocal = false;
    public int playerId = 0;
    public float speed = 5f;
    public float rotationSpeed = 40f;
    public bool isAlive = true;
    private Rigidbody rb;
    public Vector3 direction;

    public int health = 100;
    public GameObject balaPrefab;
    public GameObject ataquePrefab;
    private Animator animator;
    private bool isAttacking = false;
    private bool canRotate = true;
    public Transform puntoDeDisparo;
    public Transform puntoDeAtaque;
    public float velocidadBala = 10f;
    public float shootRate = 0.5f;
    float nextFireRate;

    public PlayerCharacter playerCharacter;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        //if (isLocal)
        //{
        //    StartCoroutine(SendPlayerPositionsToServer());
        //}
        playerCharacter = gameObject.GetComponent<PlayerCharacter>();
    }

    void Update()
    {
        if (playerCharacter != null && !playerCharacter.characterLink.isLocal)
        {
            return;
        }

        //if (isLocal)
        //{


        //Detecta los inputs
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // El movimiento de los jugadores hecho de forma que gire de forma suave
        direction = new Vector3(horizontal, 0f, vertical).normalized;


        if (direction != Vector3.zero && canRotate && !isAttacking)
        {
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);
        }

        //Al hacer click izquierdo del raton actiba la funcion de disparo
        if (Input.GetButtonDown("Fire1"))
        {

            Disparar();

        }

        if (Input.GetButtonDown("Fire2"))
        {

            Ataque();
           
        }

       // }

    }
    public void ReceiveDamage(int damage)
    {
        
        if (isAlive)
        {
            health -= damage;
            
            if (health <= 0)
            {
                
                Die();
            }
        }
    }

    public void Die()
    {
        isAlive = false;
        gameObject.SetActive(false);
    }

    void FixedUpdate()
    {
        if (!isAttacking)
        {
            rb.velocity = direction * speed;
        }
        else
        {
            rb.velocity = Vector3.zero;
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


            //if (rbBala != null)
            //{
            //    rbBala.velocity = bala.transform.forward * velocidadBala;
            //}

            //Destruye la bala 5 segundos despues de ser creada
            Destroy(bala, 5f);

        }
    }
    void Ataque()
    {
        //El if es para controlar el fire-rate
        if (Time.time > nextFireRate)
        {
            GameObject ataque = Instantiate(ataquePrefab, puntoDeAtaque.position, puntoDeAtaque.rotation);
            
            nextFireRate = Time.time + shootRate;

            if (!isAttacking)
            {
                StartCoroutine(AttackAnimation());
            }
           
            Destroy(ataque, 0.5f);

            AtaqueDamage ataqueDamage = ataque.AddComponent<AtaqueDamage>();
            ataqueDamage.SetOwner(gameObject);

        }
    }
    IEnumerator AttackAnimation()
    {
        isAttacking = true;
        canRotate = false;
        animator.SetTrigger("Attack");

        yield return new WaitForSeconds(0.45f);

        isAttacking = false;
        canRotate = true;
        animator.SetTrigger("Idle");
    }

    IEnumerator SendPlayerPositionsToServer()
    {
        while (true)
        {
            string message = "PlayerPositions,";

            Vector3 position = gameObject.transform.position;
            message += $"{gameObject.name},{position.x},{position.y},{position.z},{gameObject.transform.rotation.eulerAngles.y},{playerId}";


            //ConnectionManager.Instance.Send_Data(() => ConnectionManager.Instance.SerializeToJsonAndSend(message));


        }
    }

}
