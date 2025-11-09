using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class enemyAI : MonoBehaviour, IDamage
{
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Renderer model;
    [SerializeField] private Transform headPos;

    [SerializeField] private int HP;
    [SerializeField] private int FOV;
    [SerializeField] private int faceTargetSpeed;

    [SerializeField] private GameObject bullet;
    [SerializeField] private float shootRate;
    [SerializeField] private Transform shootPos;

    private Color colorOrig;
    private bool cowInTrigger;

    private float shootTimer;
    private float angleToCow;
    private float stoppingDistanceOrig;

    private Vector3 cowDir;

    void Start()
    {
        colorOrig = model.material.color;
        gamemanager.instance.updateGameGoal(1);
        stoppingDistanceOrig = agent.stoppingDistance;
    }

    void Update()
    {
        shootTimer += Time.deltaTime;

        if (cowInTrigger && CanSeeCow())
        {
            // The enemy AI behavior when cow is in trigger and visible
            // Move and shoot handled inside CanSeeCow()
        }
    }

    private bool CanSeeCow()
    {
        if (gamemanager.instance == null || gamemanager.instance.cow == null)
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
            StartCoroutine(FlashRed());
        }
    }

    private IEnumerator FlashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = colorOrig;
    }

    private void Shoot()
    {
        shootTimer = 0f;
        if (bullet != null && shootPos != null)
        {
            Instantiate(bullet, shootPos.position, shootPos.rotation);
        }
    }
}