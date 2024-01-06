using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    //public bool isLocal = false;
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

    public GameObject localMark;

    public bool isRunning;

    public GameObject bullet;
    public Rigidbody rbullet;

    #region sword synchronization

    public Vector3 positionToSet_Sword;
    public Vector3 rotationToSet_Sword;

    public Vector3 currentInterpolationPosition_Sword;
    public Vector3 currentInterpolationRotation_Sword;

    public float currentInterpolationTime_Sword;
    public float lastInterpolationTime_Sword;
    public float interpolationTimeDiff_Sword;

    public bool canSyncSword;

    public int syncFrameCount;

    #endregion

    private void Start()
    {
        syncFrameCount = 0;

        if (playerCharacter.characterLink.isLocal)
        {
            localMark.SetActive(true);
        }
        else
        {
            localMark.SetActive(false);
        }

        rb = GetComponent<Rigidbody>();

        animator = GetComponentInChildren<Animator>();
        //if (isLocal)
        //{
        //    StartCoroutine(SendPlayerPositionsToServer());
        //}

        haveSword = true;

        playerCharacter = gameObject.GetComponent<PlayerCharacter>();
    }

    public void SetCharacterModel(CharacterModel charModel)
    {

        if (currentCharacter != null)
        {
            Destroy(currentCharacter);
        }

        int modelInd = 0;

        switch (charModel)
        {
            case CharacterModel.None:
                modelInd = 0;
                break;

            case CharacterModel.BigHead:
                modelInd = 0;
                break;

            case CharacterModel.Gothic:
                modelInd = 1;
                break;

            case CharacterModel.Granny:
                modelInd = 2;
                break;

            case CharacterModel.CapBoy:
                modelInd = 3;
                break;
        }

        currentCharacter = Instantiate(characters[modelInd], charSpawn.position, charSpawn.rotation, transform);

        //currentCharacter.transform.parent = transform;
    }

    void Update()
    {
        if (playerCharacter != null && !playerCharacter.characterLink.isLocal)
        {
            if (canSynchronizeTransform)
            {
                CheckTransformInterpolation();
            }

            CheckTransformInterpolation_Sword();
            CheckAnimations();

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

                isRunning = true;
            }
            else
            {
                isRunning = false;
            }

            if (Input.GetButtonDown("Fire1"))
            {
                MeleeAttack meleeAttack = new MeleeAttack(playerCharacter.characterLink.playerInfo);
                meleeAttack.transferType = TransferType.AllExceptLocal;
                ConnectionManager.Instance.SerializeToJsonAndSend(meleeAttack);
                Attack();
            }

            //Al hacer click izquierdo del raton actiba la funcion de shoot
            if (Input.GetButtonDown("Fire2") && haveSword == true)
            {
                DistanceAttack distanceAttack = new DistanceAttack(playerCharacter.characterLink.playerInfo);
                distanceAttack.transferType = TransferType.AllExceptLocal;
                ConnectionManager.Instance.SerializeToJsonAndSend(distanceAttack);
                Shoot();
            }

            

        }

        CheckAnimations();
        // }

    }

    private void CheckAnimations()
    {
        if (!isAttacking)
        {
            animator.SetBool("Idle", true);
        }

        if (isRunning)
        {
            animator.SetBool("Run", true);
        }
        else
        {
            animator.SetBool("Run", false);
        }

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


        bool isMoving = true;

        if(Mathf.Abs(currentInterpolationPosition.x - transform.position.x) < 0.01 &&
           Mathf.Abs(currentInterpolationPosition.z - transform.position.z) < 0.01)
        {
            isMoving = false;
        }

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

        if (isMoving)
        {
            isRunning = true;
        }
        else
        {
            isRunning = false;
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

    public void SetTransformInterpolation_Sword(Vector3 newPos, Vector3 newRot)
    {
        if(bullet == null)
        {
            return;
        }

        syncFrameCount++;

        //Get the position and rotation to set next
        positionToSet_Sword = newPos;
        rotationToSet_Sword = newRot;

        //Get the difference between the last interpolation frame and this
        lastInterpolationTime_Sword = currentInterpolationTime_Sword;
        currentInterpolationTime_Sword = Time.time;

        interpolationTimeDiff_Sword = currentInterpolationTime_Sword - lastInterpolationTime_Sword;

        //If there is no previous interpolation frame, the difference is the default
        if (interpolationTimeDiff_Sword == 0)
        {
            interpolationTimeDiff_Sword = NetworkManager.Instance.networkUpdateInterval;
        }

        //Get the position and rotation on this frame
        currentInterpolationPosition_Sword = bullet.transform.position;
        currentInterpolationRotation_Sword = bullet.transform.eulerAngles;

        canSyncSword = true;

        CheckTransformInterpolation_Sword();
        
    }

    public void CheckTransformInterpolation_Sword()
    {
        float elapsed_time = Time.time - currentInterpolationTime_Sword;

        float interpolationDelay = 1;

        if (bullet == null || !canSyncSword)
        {
            return;
        }

        if(syncFrameCount >= 2)
        {
            bullet.SetActive(true);
        }

        switch (NetworkManager.Instance.movementInterpolation)
        {
            case InterpolationMode.None:
                {
                    bullet.transform.position = positionToSet_Sword;
                }
                break;

            case InterpolationMode.Lerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff_Sword * interpolationDelay));
                    bullet.transform.position = Vector3.Lerp(currentInterpolationPosition_Sword, positionToSet_Sword, t);

                    if(bullet.GetComponent<Bullet>().isDestroying && t >= 1)
                    {
                        Destroy(bullet);
                    }
                }
                break;

            case InterpolationMode.Slerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff_Sword * interpolationDelay));
                    bullet.transform.position = Vector3.Slerp(currentInterpolationPosition_Sword, positionToSet_Sword, t);
                }
                break;

            case InterpolationMode.SmoothStep:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff_Sword * interpolationDelay));

                    float xPos = Mathf.SmoothStep(currentInterpolationPosition_Sword.x, positionToSet_Sword.x, t);
                    float yPos = Mathf.SmoothStep(currentInterpolationPosition_Sword.y, positionToSet_Sword.y, t);
                    float zPos = Mathf.SmoothStep(currentInterpolationPosition_Sword.z, positionToSet_Sword.z, t);

                    Vector3 newPos = new Vector3(xPos, yPos, zPos);

                    bullet.transform.position = newPos;
                }
                break;
        }

        switch (NetworkManager.Instance.rotationInterpolation)
        {
            case InterpolationMode.None:
                {
                    bullet.transform.rotation = Quaternion.Euler(rotationToSet_Sword);
                }
                break;

            case InterpolationMode.Lerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff_Sword * interpolationDelay));
                    bullet.transform.rotation = Quaternion.Euler(Vector3.Lerp(currentInterpolationRotation_Sword, rotationToSet_Sword, t));
                }
                break;

            case InterpolationMode.Slerp:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff_Sword * interpolationDelay));
                    bullet.transform.rotation = Quaternion.Euler(Vector3.Slerp(currentInterpolationRotation_Sword, rotationToSet_Sword, t));
                }
                break;

            case InterpolationMode.SmoothStep:
                {
                    float t = Mathf.Clamp01(elapsed_time / (interpolationTimeDiff_Sword * interpolationDelay));

                    float xRot = Mathf.SmoothStep(currentInterpolationRotation_Sword.x, rotationToSet_Sword.x, t);
                    float yRot = Mathf.SmoothStep(currentInterpolationRotation_Sword.y, rotationToSet_Sword.y, t);
                    float zRot = Mathf.SmoothStep(currentInterpolationRotation_Sword.z, rotationToSet_Sword.z, t);

                    Vector3 newRot = new Vector3(xRot, yRot, zRot);

                    bullet.transform.rotation = Quaternion.Euler(newRot);
                }
                break;
        }

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

            bullet = Instantiate(bulletPrefab, shootPoint.position, shootPoint.rotation);
            Bullet bul = bullet.GetComponent<Bullet>();
            bul.SetOwner(gameObject);
            rbullet = bullet.GetComponent<Rigidbody>();
            if(!playerCharacter.characterLink.isLocal)
            {
                bullet.SetActive(false);
            }

            syncFrameCount = 0;

            nextFireRate = Time.time + shootRate;

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
