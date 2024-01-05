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
    public GameObject bulletPrefab;
    public GameObject attackPrefab;
    private Animator animator;
    private bool isAttacking = false;
    public bool haveSword = true;
    private bool canRotate = true;
    public Transform shootPoint;
    public Transform attackPoint;
    public Transform charSpawn;
    public float bulletSpeed = 10f;
    public float shootRate = 0.5f;
    float nextFireRate;

    public GameObject[] characters;
    private int currentCharacterIndex = 0;
    private GameObject currentCharacter;

    public PlayerCharacter playerCharacter;

    public Vector3 positionToSet;
    public Vector3 rotationToSet;

    public Vector3 currentInterpolationPosition;
    public Vector3 currentInterpolationRotation;

    public float currentInterpolationTime;
    public float lastInterpolationTime;
    public float interpolationTimeDiff;

    public bool canSynchronizeTransform;
    public bool canSendSynchronizationData;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        SwitchCharacter(currentCharacterIndex);
        animator = GetComponentInChildren<Animator>();
        //if (isLocal)
        //{
        //    StartCoroutine(SendPlayerPositionsToServer());
        //}

        haveSword = true;

        playerCharacter = gameObject.GetComponent<PlayerCharacter>();
    }

    void Update()
    {
        if (playerCharacter != null && !playerCharacter.characterLink.isLocal)
        {
            if (canSynchronizeTransform)
            {
                CheckTransformInterpolation();
            }
            return;
        }

        //if (isLocal)
        //{


        //Detecta los inputs
        if (!GameplayManager.Instance.isPaused)
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");

            // El movimiento de los jugadores hecho de forma que gire de forma suave
            direction = new Vector3(horizontal, 0f, vertical).normalized;


            if (direction != Vector3.zero && canRotate && !isAttacking)
            {
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), rotationSpeed * Time.deltaTime);

                animator.SetBool("Run", true);
            }
            else
            {
                
                animator.SetBool("Run", false);

                if (!isAttacking)
                {
                    animator.SetBool("Idle", true);
                }
            }

            //Al hacer click izquierdo del raton actiba la funcion de shoot
            if (Input.GetButtonDown("Fire1") && haveSword == true)
            {
                DistanceAttack distanceAttack = new DistanceAttack(playerCharacter.characterLink.playerInfo);
                distanceAttack.transferType = TransferType.AllExceptLocal;
                ConnectionManager.Instance.SerializeToJsonAndSend(distanceAttack);
                Shoot();
            }

            if (Input.GetButtonDown("Fire2"))
            {
                MeleeAttack meleeAttack = new MeleeAttack(playerCharacter.characterLink.playerInfo);
                meleeAttack.transferType = TransferType.AllExceptLocal;
                ConnectionManager.Instance.SerializeToJsonAndSend(meleeAttack);
                Attack();

            }

        }
       // }

    }

    public void SetTransformInterpolation(Vector3 newPos, Vector3 newRot)
    {
        canSynchronizeTransform = true;

        positionToSet = newPos;
        rotationToSet = newRot;

        lastInterpolationTime = currentInterpolationTime;
        currentInterpolationTime = Time.time;
        
        interpolationTimeDiff = currentInterpolationTime - lastInterpolationTime;

        if (interpolationTimeDiff == 0)
        {
            interpolationTimeDiff = NetworkManager.Instance.networkUpdateInterval;
        }

        currentInterpolationPosition = transform.position;
        currentInterpolationRotation = transform.eulerAngles;

        CheckTransformInterpolation();
    }

    public void CheckTransformInterpolation()
    {
        float elapsed_time = Time.time - currentInterpolationTime;

        float interpolationDelay = 1;

        switch (NetworkManager.Instance.movementInterpolation)
        {
            case InterpolationMode.None:
                {
                    transform.position = positionToSet;
                }
                break;

            case InterpolationMode.Lerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * interpolationDelay));
                    transform.position = Vector3.Lerp(currentInterpolationPosition, positionToSet, t);
                }
                break;

            case InterpolationMode.Slerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * interpolationDelay));
                    transform.position = Vector3.Slerp(currentInterpolationPosition, positionToSet, t);
                }
                break;

            case InterpolationMode.SmoothStep:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * interpolationDelay));

                    float xPos = Mathf.SmoothStep(currentInterpolationPosition.x, positionToSet.x, t);
                    float yPos = Mathf.SmoothStep(currentInterpolationPosition.y, positionToSet.y, t);
                    float zPos = Mathf.SmoothStep(currentInterpolationPosition.z, positionToSet.z, t);

                    Vector3 newPos = new Vector3(xPos, yPos, zPos);

                    transform.position = newPos;
                }
                break;
        }

        switch (NetworkManager.Instance.rotationInterpolation)
        {
            case InterpolationMode.None:
                {
                    transform.rotation = Quaternion.Euler(rotationToSet);
                }
                break;

            case InterpolationMode.Lerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * interpolationDelay));
                    transform.rotation = Quaternion.Euler(Vector3.Lerp(currentInterpolationRotation, rotationToSet, t));
                }
                break;

            case InterpolationMode.Slerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * interpolationDelay));
                    transform.rotation = Quaternion.Euler(Vector3.Slerp(currentInterpolationRotation, rotationToSet, t));
                    //transform.rotation = Vector3.Slerp(currentInterpolationRotation, rotationToSet, t);

                }
                break;

            case InterpolationMode.SmoothStep:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff * interpolationDelay));

                    float xRot = Mathf.SmoothStep(currentInterpolationRotation.x, rotationToSet.x, t);
                    float yRot = Mathf.SmoothStep(currentInterpolationRotation.y, rotationToSet.y, t);
                    float zRot = Mathf.SmoothStep(currentInterpolationRotation.z, rotationToSet.z, t);

                    Vector3 newRot = new Vector3(xRot, yRot, zRot);

                    transform.rotation = Quaternion.Euler(newRot);
                }
                break;
        }

    }

    void SwitchCharacter(int index)
    {
        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }
        
        currentCharacter = Instantiate(characters[index], charSpawn.position, charSpawn.rotation);

        currentCharacter.transform.parent = transform;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("FallenSword") && haveSword == false)
        {
            Debug.Log("Player recogió FallenSword");
            Destroy(other.gameObject);
            haveSword = true;
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
        GameplayManager.Instance.CheckEndOfRound();
        GameplayManager.Instance.CountPlayerAlive();
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

    public void Shoot()
    {
        //El if es para controlar el fire-rate
        if (Time.time > nextFireRate)
        {
            haveSword = false;

            //Crea una bullet en el point de shoot

            GameObject bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
            Rigidbody rbbullet = bullet.GetComponent<Rigidbody>();

            nextFireRate = Time.time + shootRate;


            //if (rbbullet != null)
            //{
            //    rbbullet.velocity = bullet.transform.forward * bulletSpeed;
            //}

            //Destruye la bullet 5 segundos despues de ser creada
            Destroy(bullet, 5f);

        }
    }
    public void Attack()
    {
        //El if es para controlar el fire-rate
        if (Time.time > nextFireRate)
        {
            GameObject Attack = Instantiate(attackPrefab, attackPoint.position, attackPoint.rotation);
            AttackDamage AttackDamageScript = Attack.GetComponent<AttackDamage>();
            if (AttackDamageScript != null)
            {
                AttackDamageScript.SetOwner(gameObject);
            }
            animator.SetTrigger("Attack");
            nextFireRate = Time.time + shootRate;

            if (!isAttacking)
            {
                StartCoroutine(AttackAnimation());
            }
           
            Destroy(Attack, 0.5f);

            AttackDamage AttackDamage = Attack.AddComponent<AttackDamage>();
            AttackDamage.SetOwner(gameObject);

        }
    }
    IEnumerator AttackAnimation()
    {
        isAttacking = true;
        canRotate = false;
        

        yield return new WaitForSeconds(0.45f);

        isAttacking = false;
        canRotate = true;
        //animator.SetBool("Attack", false);
        //animator.SetTrigger("Idle");
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
