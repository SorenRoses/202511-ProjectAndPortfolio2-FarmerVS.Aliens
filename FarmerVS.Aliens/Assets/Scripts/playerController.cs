using UnityEngine;
using System.Collections;

public class playerController : MonoBehaviour, IDamage
{
    [SerializeField] LayerMask ignoreLayer;

    [SerializeField] CharacterController controller;

    [SerializeField] int HP;
    [SerializeField] float speed;         // Movement speed
    [SerializeField] float sprintMod;    // Sprint speed multiplier
    [SerializeField] float jumpSpeed;    // Initial jump velocity
    [SerializeField] int jumpCountMax;   // Max allowed jumps (e.g., double jump)
    [SerializeField] float gravity;      // Gravity acceleration

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
        // Reset jump count and vertical velocity if grounded
        if (controller.isGrounded)
        {
            playerVel.y = -2f;  // Small downward force to keep grounded
            jumpCount = 0;
        }
        else
        {
            playerVel.y -= gravity * Time.deltaTime;  // Apply gravity while in air
        }

        // Get input for horizontal and vertical movement
        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        // Calculate movement direction relative to player orientation
        moveDir = inputX * transform.right + inputZ * transform.forward;
        controller.Move(moveDir * speed * Time.deltaTime);

        // Handle jumping input
        jump();

        // Apply vertical velocity (gravity and jump) to character controller
        controller.Move(playerVel * Time.deltaTime);

        // Update animator Speed parameter based on movement magnitude
        float moveMagnitude = new Vector3(inputX, 0, inputZ).magnitude;
        if (animator != null)
        {
            animator.SetFloat("Speed", moveMagnitude);
            // Removed animator.SetBool("IsJumping", ...) to avoid errors
        }

        // Shooting input and logic
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
            if (animator != null) animator.SetBool("IsSprinting", true);
        }
        else if (Input.GetButtonUp("Sprint") && isSprinting)
        {
            speed = baseSpeed;
            isSprinting = false;
            if (animator != null) animator.SetBool("IsSprinting", false);
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

        if (animator != null) animator.SetTrigger("Shoot");
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
