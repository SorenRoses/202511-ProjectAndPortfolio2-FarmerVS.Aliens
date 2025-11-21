using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Cow : MonoBehaviour, IDamage
{
    
    [SerializeField] private int HP;
    [SerializeField] private float wanderRadius;
    [SerializeField] private float wanderDelay;
    [SerializeField] private float fleeDistance;
    [SerializeField] private float fleeSpeed;

    private NavMeshAgent agent;
    private Renderer model;
    private Color originalColor;

    private bool isFleeing = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        model = GetComponentInChildren<Renderer>();
        if (model != null)
            originalColor = model.sharedMaterial.color;
    }

    void Start()
    {
        StartCoroutine(WanderRoutine());
    }

    IEnumerator WanderRoutine()
    {
        while (true)
        {
            if (!isFleeing)
            {
                Vector3 newPos = RandomNavSphere(transform.position, wanderRadius);
                agent.speed = agent.speed; 
                agent.SetDestination(newPos);
            }
            yield return new WaitForSeconds(wanderDelay);
        }
    }

    Vector3 RandomNavSphere(Vector3 origin, float dist)
    {
        Vector3 randDirection = Random.insideUnitSphere * dist;
        randDirection += origin;
        NavMeshHit navHit;
        NavMesh.SamplePosition(randDirection, out navHit, dist, NavMesh.AllAreas);
        return navHit.position;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        if (HP <= 0)
        {
            gamemanager.instance?.updateGameGoal(-1);
            gamemanager.instance?.UpdateCowCount(-1);
            Destroy(gameObject);
            return;
        }

        StartCoroutine(FlashRed());
        StartFleeing();
    }


        IEnumerator FlashRed()
    {
        if (model != null)
        {
            model.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            model.material.color = originalColor;
        }
    }

    void StartFleeing()
    {
        if (!isFleeing)
        {
            isFleeing = true;
            StopCoroutine(WanderRoutine());
            StartCoroutine(FleeRoutine());
        }
    }

    IEnumerator FleeRoutine()
    {
        float fleeTime = 3f; 
        float timer = 0f;

        while (timer < fleeTime)
        {
            Transform enemy = FindClosestEnemy();
            if (enemy != null)
            {
                Vector3 fleeDir = (transform.position - enemy.position).normalized;
                Vector3 fleePos = transform.position + fleeDir * fleeDistance;

                NavMeshHit navHit;
                if (NavMesh.SamplePosition(fleePos, out navHit, fleeDistance, NavMesh.AllAreas))
                    agent.SetDestination(navHit.position);

                agent.speed = fleeSpeed;
            }

            timer += Time.deltaTime;
            yield return null;
        }

        isFleeing = false;
        StartCoroutine(WanderRoutine());
    }

    Transform FindClosestEnemy()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        Transform closest = null;
        float minDist = float.MaxValue;

        foreach (GameObject e in enemies)
        {
            float dist = Vector3.Distance(transform.position, e.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                closest = e.transform;
            }
        }

        return closest;
    }
}