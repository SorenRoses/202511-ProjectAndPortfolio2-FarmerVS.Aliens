using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPos;
    [SerializeField] private int HP = 100;
    [SerializeField] private int faceTargetSpeed = 5;
    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootRate = 0.3f;
    [SerializeField] private Transform shootPos;

    private Color colorOrig = Color.white;
    private float shootTimer;
    private Animator animator;
    private static readonly int ShootParam = Animator.StringToHash("Shoot");
    private Transform player;
    private Transform[] cows;
    private Transform target;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (model == null) model = GetComponentInChildren<Renderer>();
        if (headPos == null) headPos = transform;

        animator = GetComponent<Animator>();

        if (model != null && model.sharedMaterial != null)
            colorOrig = model.sharedMaterial.color;
    }

    void Start()
    {
        if (gamemanager.instance != null)
        {
            player = gamemanager.instance.Player?.transform;
            cows = GameObject.FindGameObjectsWithTag("Cow")
                .Select(c => c.transform)
                .ToArray();
            gamemanager.instance.updateGameGoal(1);
        }
    }

    void Update()
    {
        if (agent == null) return;

        shootTimer += Time.deltaTime;

        SelectClosestTarget();

        if (target != null)
        {
            agent.SetDestination(target.position);

            Vector3 lookDir = target.position - transform.position;
            lookDir.y = 0;

            if (lookDir.sqrMagnitude > 0.001f)
            {
                Quaternion targetRot = Quaternion.LookRotation(lookDir);
                transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, faceTargetSpeed * Time.deltaTime);
            }

            if (shootTimer >= shootRate)
            {
                ShootTarget();
            }
        }
    }

    void SelectClosestTarget()
    {
        target = player;
        float minDist = player != null ? Vector3.Distance(transform.position, player.position) : float.MaxValue;

        if (cows != null)
        {
            foreach (Transform c in cows)
            {
                if (c == null) continue;
                float dist = Vector3.Distance(transform.position, c.position);
                if (dist < minDist)
                {
                    target = c;
                    minDist = dist;
                }
            }
        }
    }

    private void ShootTarget()
    {
        shootTimer = 0f;
        if (bullet == null || shootPos == null || target == null) return;

        Collider targetCollider = target.GetComponent<Collider>();
        Vector3 targetCenter = (targetCollider != null) ? targetCollider.bounds.center : target.position;

        Vector3 shootDir = targetCenter - shootPos.position;
        if (shootDir.sqrMagnitude > 0.001f)
        {
            Quaternion bulletRot = Quaternion.LookRotation(shootDir);
            Instantiate(bullet, shootPos.position, bulletRot);
        }

        if (animator != null)
            animator.SetBool(ShootParam, true);
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            gamemanager.instance?.updateGameGoal(-1);
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
}