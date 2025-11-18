using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Animator anim;
    [SerializeField] Renderer model;
    [SerializeField] Transform headPos;

    [SerializeField] int HP;
    [SerializeField] int FOV;
    [SerializeField] int faceTargetSpeed;
    [SerializeField] int roamDist;
    [SerializeField] int roamPauseTime;
    [SerializeField] int animTranSpeed;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;

    Color colorOrig;

    bool cowInTrigger;

    float shootTimer;
    float roamTimer;
    float angleToPlayer;
    float stoppingDistOrig;

    static readonly int ShootParam = Animator.StringToHash("Shoot");
    private Transform player;
    private Transform[] cows;
    private Transform target;

    Vector3 startingPos;

    Vector3 playerDir;

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (model == null) model = GetComponentInChildren<Renderer>();
        if (headPos == null) headPos = transform;

        anim = GetComponent<Animator>();

        if (model != null && model.sharedMaterial != null)
            colorOrig = model.sharedMaterial.color;
    }

    void Start()
    {

        startingPos = transform.position;
        player = gamemanager.instance.player.transform;
        cows = gamemanager.instance.GetCowTransforms();
        gamemanager.instance.updateGameGoal(1);
        stoppingDistOrig = agent.stoppingDistance;
        startingPos = transform.position;
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        float agentSpeedCur = agent.velocity.normalized.magnitude;
        float agentSpeedAnim = anim.GetFloat("Speed");

        anim.SetFloat("Speed", Mathf.Lerp(agentSpeedAnim, agentSpeedCur, Time.deltaTime * animTranSpeed));


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

    void checkRoam()
    {
        if (agent.remainingDistance < 0.01f && roamTimer >= roamPauseTime)
        {
            roam();
        }
    }

    void roam()
    {
        roamTimer = 0;
        agent.stoppingDistance = 0;

        Vector3 ranPos = Random.insideUnitSphere * roamDist;
        ranPos += startingPos;

        NavMeshHit hit;
        NavMesh.SamplePosition(ranPos, out hit, roamDist, 1);
        agent.SetDestination(hit.position);
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

        if (anim != null)
            anim.SetBool(ShootParam, true);
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