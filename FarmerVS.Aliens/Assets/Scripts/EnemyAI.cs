using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [Header("Components")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator anim;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPos;
    [SerializeField] private Collider weaponCol;

    [Header("Stats")]
    [SerializeField] private int HP;
    [SerializeField] private int faceTargetSpeed;
    [SerializeField] private int FOV;
    [SerializeField] private int roamDist;
    [SerializeField] private float roamPauseTime;
    [SerializeField] private float animTransSpeed;

    [Header("Shooting")]
    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootRate;
    [SerializeField] private Transform shootPos;

    private Color colorOrig;
    private float shootTimer;
    private float roamTimer;
    private float angleToTarget;
    private float stoppingDistOrig;

    private Transform player;
    private Transform[] cows;
    private Transform target;
    private Vector3 startingPos;
    private Vector3 playerDir;
    private bool playerInTrigger;

    private static readonly int ShootParam = Animator.StringToHash("Shoot");

    void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (model == null) model = GetComponentInChildren<Renderer>();
        if (headPos == null) headPos = transform;
        if (anim == null) anim = GetComponent<Animator>();
    }

    void Start()
    {
        startingPos = transform.position;
        stoppingDistOrig = agent.stoppingDistance;

        if (model != null && model.material != null)
            colorOrig = model.material.color;

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
        shootTimer += Time.deltaTime;

        float agentSpeedCur = agent.velocity.magnitude;
        float agentSpeedAnim = anim.GetFloat("Speed");
        anim.SetFloat("Speed", Mathf.Lerp(agentSpeedAnim, agentSpeedCur, Time.deltaTime * animTransSpeed));

        
        SelectClosestTarget();

        if (target == null || (target == player && !CanSeePlayer()))
        {
            CheckRoam();
        }
        else
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
                ShootTarget();
        }
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

    bool CanSeePlayer()
    {
        if (player == null) return false;

        playerDir = player.position - headPos.position;
        angleToTarget = Vector3.Angle(playerDir, transform.forward);

        if (angleToTarget > FOV) return false;

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, playerDir, out hit))
        {
            if (hit.collider.CompareTag("Player"))
            {
                agent.SetDestination(player.position);
                agent.stoppingDistance = stoppingDistOrig;

                if (shootTimer >= shootRate)
                    ShootTarget();

                return true;
            }
        }

        agent.stoppingDistance = 0;
        return false;
    }

    void CheckRoam()
    {
        roamTimer += Time.deltaTime;
        if (agent.remainingDistance < 0.01f && roamTimer >= roamPauseTime)
        {
            Roam();
        }
    }

    void Roam()
    {
        roamTimer = 0f;
        agent.stoppingDistance = 0;
        Vector3 ranPos = Random.insideUnitSphere * roamDist + startingPos;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(ranPos, out hit, roamDist, NavMesh.AllAreas))
        {
            agent.SetDestination(hit.position);
        }
    }

    void ShootTarget()
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

        if (anim != null)
            anim.SetTrigger("Shoot");
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
        if (model != null && model.material != null)
        {
            model.material.color = Color.red;
            yield return new WaitForSeconds(0.1f);
            model.material.color = colorOrig;
        }
    }


    public void weaponColOn()
    { weaponCol.enabled = true; }
    public void weaponColOff() 
    { weaponCol.enabled = false; }
}