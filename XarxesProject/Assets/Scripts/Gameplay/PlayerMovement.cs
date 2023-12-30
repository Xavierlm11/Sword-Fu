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
    public int wins = 0;

    public int health = 100;
    public GameObject balaPrefab;
    public GameObject ataquePrefab;
    private Animator animator;
    private bool isAttacking = false;
    public bool havesword = true;
    private bool canRotate = true;
    public Transform puntoDeDisparo;
    public Transform puntoDeAtaque;
    public float velocidadBala = 10f;
    public float shootRate = 0.5f;
    float nextFireRate;

    public PlayerCharacter playerCharacter;

    public Vector3 positionToSet;
    public float currentInterpolationTime;
    public float lastInterpolationTime;
    public float interpolationTimeDiff;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
        //if (isLocal)
        //{
        //    StartCoroutine(SendPlayerPositionsToServer());
        //}

        havesword = true;

        playerCharacter = gameObject.GetComponent<PlayerCharacter>();
    }

    void Update()
    {
        if (playerCharacter != null && !playerCharacter.characterLink.isLocal)
        {
            CheckTransformInterpolation();
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
        if (Input.GetButtonDown("Fire1") && havesword == true)
        {

            Disparar();

        }

        if (Input.GetButtonDown("Fire2"))
        {

            Ataque();
           
        }

       // }

    }

    public void SetTransformInterpolation(Vector3 newPos)
    {
        positionToSet = newPos;

        lastInterpolationTime = currentInterpolationTime;
        currentInterpolationTime = Time.time;
        
        interpolationTimeDiff = currentInterpolationTime - lastInterpolationTime;

        if (interpolationTimeDiff == 0)
        {
            interpolationTimeDiff = NetworkManager.Instance.networkUpdateInterval;
        }


        CheckTransformInterpolation();
    }

    public void CheckTransformInterpolation()
    {
        float elapsed_time = Time.time - currentInterpolationTime;

        switch (NetworkManager.Instance.movementInterpolation)
        {
            case InterpolationMode.None:
                {
                    transform.position = positionToSet;
                }
                break;

            case InterpolationMode.Lerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * 2));
                    transform.position = Vector3.Lerp(transform.position, positionToSet, t);
                }
                break;

            case InterpolationMode.Slerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * 2));
                    transform.position = Vector3.Slerp(transform.position, positionToSet, t);
                }
                break;

            case InterpolationMode.SmoothStep:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * 2));

                    float xPos = Mathf.SmoothStep(transform.position.x, positionToSet.x, t);
                    float yPos = Mathf.SmoothStep(transform.position.y, positionToSet.y, t);
                    float zPos = Mathf.SmoothStep(transform.position.z, positionToSet.z, t);

                    Vector3 newPos = new Vector3(xPos, yPos, zPos);

                    transform.position = newPos;
                }
                break;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallenSword") && havesword == false)
        {
            Debug.Log("Player recogió FallenSword");
            Destroy(other.gameObject);
            havesword = true;
        }
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
            havesword = false;

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
