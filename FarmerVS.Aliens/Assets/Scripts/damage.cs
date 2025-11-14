using UnityEngine;
using System.Collections;

public class damage : MonoBehaviour
{
    enum damageType { moving, stationary, dot, homing }
    [SerializeField] damageType type;
    [SerializeField] Rigidbody rb;

    [SerializeField] int damageAmount;
    [SerializeField] float damageRate;
    [SerializeField] float speed;
    [SerializeField] float destroyTime;

    bool isDamaging;

    void Start()
    {
        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject, destroyTime);

            if (type == damageType.moving)
            {
                if (rb != null)
                    rb.linearVelocity = transform.forward * speed;
            }
        }
    }

    void Update()
    {
        if (type == damageType.homing)
        {
            if (rb != null && gamemanager.instance != null && gamemanager.instance.Player != null)
            {
                Vector3 direction = (gamemanager.instance.Player.transform.position - transform.position).normalized;
                rb.linearVelocity = direction * speed;
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type == damageType.dot)
        {
            dmg.takeDamage(damageAmount);
        }

        if (type == damageType.moving || type == damageType.homing)
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.isTrigger)
            return;

        IDamage dmg = other.GetComponent<IDamage>();

        if (dmg != null && type == damageType.dot && !isDamaging)
        {
            StartCoroutine(DamageOtherCoroutine(dmg));
        }
    }

    IEnumerator DamageOtherCoroutine(IDamage d)
    {
        isDamaging = true;
        d.takeDamage(damageAmount);
        yield return new WaitForSeconds(damageRate);
        isDamaging = false;
    }
}
