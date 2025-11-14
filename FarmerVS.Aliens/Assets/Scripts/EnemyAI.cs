using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPos;

    [SerializeField] private int HP = 100;
    [SerializeField] private int FOV = 90;
    [SerializeField] private int faceTargetSpeed = 5;

    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootRate = 1f;
    [SerializeField] private Transform shootPos;

    private Color colorOrig = Color.white;
    private bool cowInTrigger;

    private float shootTimer;
    private float angleToCow;
    private float stoppingDistanceOrig;

    private Vector3 cowDir;

    private Animator animator;
    private static readonly int ShootParam = Animator.StringToHash("Shoot");

    void Awake()
    {
        // Auto-assign if not assigned in inspector, with warnings
        if (agent == null)
        {
            agent = GetComponent<NavMeshAgent>();
            if (agent == null)
                Debug.LogError("NavMeshAgent component missing on enemyAI GameObject.");
        }

        if (model == null)
        {
            model = GetComponentInChildren<Renderer>();
            if (model == null)
                Debug.LogWarning("Renderer not assigned and no child renderer found.");
        }

        if (headPos == null)
        {
            headPos = transform; // fallback to own transform if no headPos assigned
            Debug.LogWarning("headPos not assigned, defaulting to transform.");
        }

        animator = GetComponent<Animator>();
        if (animator == null)
            Debug.LogWarning("Animator component missing on enemyAI GameObject.");
    }

    void Start()
    {
        if (model != null && model.sharedMaterial != null)
            colorOrig = model.sharedMaterial.color;

        if (gamemanager.instance != null)
            gamemanager.instance.updateGameGoal(1);

        if (agent != null)
            stoppingDistanceOrig = agent.stoppingDistance;
    }

    void Update()
    {
        if (agent == null)
            return;

        shootTimer += Time.deltaTime;

        if (cowInTrigger && CanSeeCow())
        {
            // Shooting and facing handled inside CanSeeCow()
        }
        else
        {
            if (animator != null && animator.GetBool(ShootParam))
            {
                animator.SetBool(ShootParam, false);
            }
        }
    }

    private bool CanSeeCow()
    {
        if (gamemanager.instance == null || gamemanager.instance.cow == null || headPos == null)
            return false;

        cowDir = gamemanager.instance.cow.transform.position - headPos.position;
        angleToCow = Vector3.Angle(cowDir, transform.forward);

        Debug.DrawRay(headPos.position, cowDir, Color.green);

        if (angleToCow > FOV)
            return false;

        if (Physics.Raycast(headPos.position, cowDir.normalized, out RaycastHit hit))
        {
            if (hit.collider.CompareTag("Cow"))
            {
                agent.SetDestination(gamemanager.instance.cow.transform.position);

                if (shootTimer >= shootRate)
                {
                    Shoot();
                }

                if (agent.remainingDistance <= stoppingDistanceOrig)
                {
                    FaceTarget();
                }
                return true;
            }
        }
        return false;
    }

    private void FaceTarget()
    {
        Vector3 lookDirection = new Vector3(cowDir.x, 0, cowDir.z);
        if (lookDirection.sqrMagnitude > 0.001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(lookDirection);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, faceTargetSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Cow"))
        {
            cowInTrigger = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Cow"))
        {
            cowInTrigger = false;
            if (animator != null)
                animator.SetBool(ShootParam, false);
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            if (gamemanager.instance != null)
                gamemanager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(FlashRed());
        }
    }

    private IEnumerator FlashRed()
    {
        if (model != null && model.sharedMaterial != null)
        {
            model.sharedMaterial.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            model.sharedMaterial.color = colorOrig;
        }
    }

    private void Shoot()
    {
        shootTimer = 0f;

        if (bullet != null && shootPos != null)
        {
            Instantiate(bullet, shootPos.position, shootPos.rotation);
        }

        if (animator != null)
        {
            animator.SetBool(ShootParam, true);
        }
    }
}
