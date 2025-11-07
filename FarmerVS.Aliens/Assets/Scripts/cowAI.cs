using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.AI;


public class cowAI : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform headPos;

    [SerializeField] int HP;
    [SerializeField] int FOV;
    [SerializeField] int faceTargetSpeed;

    [SerializeField] float fleeDistance;


    Color colorOrig;

    bool enemyInTrigger;


    float angleToEnemy;
    float stoppingDistanceOrig;

    Vector3 enemyDir;

    Transform seenEnemy;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        colorOrig = model.material.color;
        gamemanager.instance.updateGameGoal(1);
        stoppingDistanceOrig = agent.stoppingDistance;
    }

    // Update is called once per frame
    void Update()
    {

        if (enemyInTrigger && canSeeEnemy())
        {
            runAway();
        }
    }

    bool canSeeEnemy()
    {
        if (seenEnemy == null)
            return false;

        enemyDir = seenEnemy.position - headPos.position;
        angleToEnemy = Vector3.Angle(enemyDir, transform.forward);

        Debug.DrawRay(headPos.position, enemyDir);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, enemyDir, out hit))
        {
            Debug.Log(hit.collider.name);

            if (angleToEnemy <= FOV && hit.collider.CompareTag("Enemy"))
            {
                
                return true;
            }
        }
        return false;
    }

    void runAway()
    {
        Vector3 dirAway = transform.position - seenEnemy.position;
   
        Vector3 fleeTarget = transform.position + dirAway * fleeDistance;
        agent.SetDestination(fleeTarget);

        if (agent.remainingDistance <= stoppingDistanceOrig)
            faceTarget();
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(enemyDir.x, transform.position.y, enemyDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyInTrigger = true;
            seenEnemy = other.transform;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            enemyInTrigger = false;
            seenEnemy = null;
        }
    }

    public void takeDamage(int amount)
    {
        HP -= amount;

        if (HP <= 0)
        {
            gamemanager.instance.updateGameGoal(-1);
            Destroy(gameObject);
        }
        else
        {
            StartCoroutine(flashRed());
        }
    }

    IEnumerator flashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }


   
}