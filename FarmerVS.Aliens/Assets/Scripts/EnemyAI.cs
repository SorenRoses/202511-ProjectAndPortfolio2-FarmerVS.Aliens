using UnityEngine;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] NavMeshAgent agent;
    [SerializeField] Renderer model;
    [SerializeField] Transform headPos;

    [SerializeField] int HP;
    [SerializeField] int FOV;
    [SerializeField] int faceTargetSpeed;

    [SerializeField] GameObject bullet;
    [SerializeField] float shootRate;
    [SerializeField] Transform shootPos;

    Color colorOrig;

    bool cowInTrigger;

    float shootTimer;
    float angleToCow;
    float stoppingDistanceOrig;

    Vector3 cowDir;

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
        shootTimer += Time.deltaTime;

        if (cowInTrigger && canSeeCow())
        {

        }
    }

    bool canSeeCow()
    {
        cowDir = gamemanager.instance.cow.transform.position - headPos.position;
        angleToCow = Vector3.Angle(cowDir, transform.forward);

        Debug.DrawRay(headPos.position, cowDir);

        RaycastHit hit;
        if (Physics.Raycast(headPos.position, cowDir, out hit))
        {
            Debug.Log(hit.collider.name);

            if (angleToCow <= FOV && hit.collider.CompareTag("Cow"))
            {
                agent.SetDestination(gamemanager.instance.cow.transform.position);

                if (shootTimer >= shootRate)
                {
                    shoot();
                }

                if (agent.remainingDistance <= stoppingDistanceOrig)
                    faceTarget();

                return true;
            }
        }
        return false;
    }

    void faceTarget()
    {
        Quaternion rot = Quaternion.LookRotation(new Vector3(cowDir.x, transform.position.y, cowDir.z));
        transform.rotation = Quaternion.Lerp(transform.rotation, rot, faceTargetSpeed * Time.deltaTime);
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

    void shoot()
    {
        shootTimer = 0;
        Instantiate(bullet, shootPos.position, transform.rotation);
    }
}