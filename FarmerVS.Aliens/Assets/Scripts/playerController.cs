using UnityEngine;
using System.Collections;
using System.IO;


public class playerController : MonoBehaviour, IDamage
{
    [Header ("----- Components -----")]
    [SerializeField] CharacterController controller;
    [SerializeField] LayerMask ignoreLayer;

    [Header("----- Stats -----")]
    [Range (1, 10)] [SerializeField] int HP;
    [Range(3, 6)] [SerializeField] float speed;
    [Range(2, 5)] [SerializeField] float sprintMod;
    [Range(5, 20)] [SerializeField] float jumpSpeed;
    [Range(1, 3)] [SerializeField] int jumpCountMax;
    [Range(15, 50)] [SerializeField] float gravity;


    [Header("----- Guns -----")]
    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    [Header("----- Audio -----")]
    [SerializeField] AudioSource aud;
    [SerializeField] AudioClip[] audStep;
    [Range(0, 1)] [SerializeField] float audStepVol;
    [SerializeField] AudioClip[] audJump;
    [Range(0, 1)] [SerializeField] float audJumpVol;
    [SerializeField] AudioClip[] audHurt;
    [Range(0, 1)] [SerializeField] float audHurtVol;



    [SerializeField] GameObject playerDamagePanel;

    Vector3 moveDir;
    Vector3 playerVel;

    int jumpCount;
    int HPOrig;
    float shootTimer;
    bool isSprinting;
    bool isShooting;
    bool isPlayingStep;

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
            if (moveDir.normalized.magnitude > 0.3f && !isPlayingStep)
            {
                StartCoroutine(playStep());
            }

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

    IEnumerator playStep()
    {
        isPlayingStep = true;
        aud.PlayOneShot(audStep[Random.Range(0, audStep.Length)], audStepVol);

        if (isSprinting)
        {
            yield return new WaitForSeconds(0.3f);
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
        }

        isPlayingStep = false;
    }


    void Jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpCountMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
            aud.PlayOneShot(audJump[Random.Range(0, audJump.Length)], audJumpVol);
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
            animator.SetBool("isSprinting", isSprinting);
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
        aud.PlayOneShot(audHurt[Random.Range(0, audHurt.Length)], audHurtVol);

        if (HP <= 0)
        {
            gamemanager.instance.youLose();
        }
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