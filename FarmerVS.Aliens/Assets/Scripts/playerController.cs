using System.Collections;
using System.Collections.Generic;
using NUnit.Framework.Interfaces;
using UnityEngine;

public class playerController : MonoBehaviour, IDamage
{
   
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] int HP;
    [SerializeField] float speed;
    [SerializeField] float sprintMod;
    [SerializeField] float jumpSpeed;
    [SerializeField] int jumpCountMax;
    [SerializeField] float gravity;

    [SerializeField] List<gunStats> gunList = new List<gunStats>();
    [SerializeField] GameObject gunModel;
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    [SerializeField] GameObject playerDamagePanel;

    Vector3 moveDir;
    Vector3 playerVel;

    int jumpCount;
    int HPOrig;
    float shootTimer;
    bool isSprinting;
    bool isShooting;

    Animator animator;
    float baseSpeed;

    void Start()
    {
        HPOrig = HP;
        animator = GetComponent<Animator>();
        baseSpeed = speed;
        UpdatePlayerUI();
    }

    void Update()
    {
        if (gamemanager.instance != null && gamemanager.instance.isPaused) return;

        shootTimer += Time.deltaTime;

        HandleMovement();
        HandleSprinting();
        HandleShooting();
    }

    void HandleMovement()
    {
        
        if (controller.isGrounded)
        {
            playerVel.y = -2f;
            jumpCount = 0;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        moveDir = transform.right * inputX + transform.forward * inputZ;
        controller.Move(moveDir * speed * Time.deltaTime);

        Jump();

        controller.Move(playerVel * Time.deltaTime);

       
        if (animator != null)
        {
            float moveMag = new Vector3(inputX, 0, inputZ).magnitude;
            animator.SetFloat("Speed", moveMag);
        }
    }

    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpCountMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void HandleSprinting()
    {
        if (Input.GetButton("Sprint"))
        {
            speed = baseSpeed * sprintMod;
            isSprinting = true;
        }
        else
        {
            speed = baseSpeed;
            isSprinting = false;
        }

        if (animator != null)
            animator.SetBool("IsSprinting", isSprinting);
    }

    void HandleShooting()
    {
        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shootTimer = 0f;

            RaycastHit hit;
            if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
            {
                IDamage dmg = hit.collider.GetComponent<IDamage>();
                if (dmg != null)
                    dmg.takeDamage(shootDamage);
            }

            if (animator != null)
            {
                animator.SetBool("Shoot", true);
                isShooting = true;
            }
        }

        
        if (!Input.GetButton("Fire1") && isShooting)
        {
            isShooting = false;
            if (animator != null)
                animator.SetBool("Shoot", false);
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        UpdatePlayerUI();

        StartCoroutine(ScreenFlashDamage());

        if (HP <= 0)
            gamemanager.instance.youLose();
    }

    void UpdatePlayerUI()
    {
        if (gamemanager.instance != null && gamemanager.instance.PlayerHPBar != null)
            gamemanager.instance.PlayerHPBar.fillAmount = (float)HP / HPOrig;
    }

    IEnumerator ScreenFlashDamage()
    {
        if (playerDamagePanel != null)
        {
            playerDamagePanel.SetActive(true);
            yield return new WaitForSeconds(0.1f);
            playerDamagePanel.SetActive(false);
        }
    }
}