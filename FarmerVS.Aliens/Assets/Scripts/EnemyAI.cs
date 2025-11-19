using System.Collections;
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

            GameObject[] cowObjects = GameObject.FindGameObjectsWithTag("Cow");
            cows = new Transform[cowObjects.Length];
            for (int i = 0; i < cowObjects.Length; i++)
                cows[i] = cowObjects[i].transform;

            gamemanager.instance.updateGameGoal(1);
        }
    }

    void Update()
    {
        if (agent == null) return;

        shootTimer += Time.deltaTime;

        SelectClosestTarget();

        if (target == null) return;

        agent.SetDestination(target.position);

        Vector3 lookDir = target.position - transform.position;
        lookDir.y = 0;

        if (lookDir.sqrMagnitude > 0.001f)
        {
            Quaternion targetRot = Quaternion.LookRotation(lookDir);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRot, faceTargetSpeed * Time.deltaTime);
        }

        if (shootTimer >= shootRate)
            ShootTarget();
    }

    void SelectClosestTarget()
    {
        target = null;
        float minDist = float.MaxValue;

        // Check player first  
        if (player != null)
        {
            float dist = Vector3.Distance(transform.position, player.position);
            if (dist < minDist)
            {
                target = player;
                minDist = dist;
            }
        }

        // Check cows  
        if (cows != null)
        {
            for (int i = 0; i < cows.Length; i++)
            {
                Transform c = cows[i];
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

        Collider targetCol = target.GetComponent<Collider>();
        Vector3 targetPos = (targetCol != null) ? targetCol.bounds.center : target.position;

        Vector3 shootDir = targetPos - shootPos.position;
        if (shootDir.sqrMagnitude > 0.001f)
        {
            Quaternion rot = Quaternion.LookRotation(shootDir);
            Instantiate(bullet, shootPos.position, rot);
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