using UnityEngine;
using System.Collections;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] CharacterController controller;

    [SerializeField] int HP;
    [SerializeField] float speed;         // Changed to float for smooth movement
    [SerializeField] float sprintMod;     // Changed to float
    [SerializeField] float jumpSpeed;     // Changed to float
    [SerializeField] int jumpCountMax;
    [SerializeField] float gravity;       // Changed to float

    [SerializeField] int shootDamage;
    [SerializeField] int shootDist;
    [SerializeField] float shootRate;

    Vector3 moveDir;
    Vector3 playerVel;

    int jumpCount;
    int HPOrig;

    float shootTimer;

    bool isSprinting;

    Animator animator;  // Animator reference

    float baseSpeed;

    void Start()
    {
        HPOrig = HP;
        updatePlayerUI();

        animator = GetComponent<Animator>();  // Get Animator component
        baseSpeed = speed;
    }

    void Update()
    {
        Debug.DrawRay(Camera.main.transform.position, Camera.main.transform.forward * shootDist, Color.red, ~ignoreLayer);
        shootTimer += Time.deltaTime;

        movement();

        sprint();
    }

    void movement()
    {
        if (controller.isGrounded)
        {
            playerVel = Vector3.zero;
            jumpCount = 0;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;
        }

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        moveDir = inputX * transform.right + inputZ * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        jump();
        controller.Move(playerVel * Time.deltaTime);

        // Update animator Speed parameter based on movement magnitude
        float moveMagnitude = new Vector3(inputX, 0, inputZ).magnitude;
        animator.SetFloat("Speed", moveMagnitude);

        animator.SetBool("IsJumping", !controller.isGrounded);

        if (Input.GetButton("Fire1") && shootTimer >= shootRate)
        {
            shoot();
        }
    }

    void sprint()
    {
        if (Input.GetButtonDown("Sprint") && !isSprinting)
        {
            speed = baseSpeed * sprintMod;
            isSprinting = true;
            animator.SetBool("IsSprinting", true);
        }
        else if (Input.GetButtonUp("Sprint") && isSprinting)
        {
            speed = baseSpeed;
            isSprinting = false;
            animator.SetBool("IsSprinting", false);
        }
    }

    void jump()
    {
        if (Input.GetButtonDown("Jump") && jumpCount < jumpCountMax)
        {
            playerVel.y = jumpSpeed;
            jumpCount++;
        }
    }

    void shoot()
    {
        shootTimer = 0;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, shootDist, ~ignoreLayer))
        {
            Debug.Log(hit.collider.name);

            IDamage dmg = hit.collider.GetComponent<IDamage>();
            if (dmg != null)
            {
                dmg.takeDamage(shootDamage);
            }
        }

        animator.SetTrigger("Shoot");
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        updatePlayerUI();
        StartCoroutine(screenFlashDamage());

        if (HP <= 0)
        {
            // YOU LOSE!!
            gamemanager.instance.youLose();
        }
    }

    public void updatePlayerUI()
    {
        gamemanager.instance.PlayerHPBar.fillAmount = (float)HP / HPOrig;
    }

    IEnumerator screenFlashDamage()
    {
        gamemanager.instance.PlayerDamagePanel.SetActive(true);
        yield return new WaitForSeconds(0.1f);
        gamemanager.instance.PlayerDamagePanel.SetActive(false);
    }
}
