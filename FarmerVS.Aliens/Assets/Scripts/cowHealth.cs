using UnityEngine;
using System.Collections;

public class cowHealth : MonoBehaviour, IDamage
{
    [SerializeField] private int HP = 10;
    [SerializeField] private Renderer model;

    private Color originalColor;

    void Awake()
    {
        if (model == null)
            model = GetComponentInChildren<Renderer>();

        if (model != null)
            originalColor = model.sharedMaterial.color;
    }

    public void takeDamage(int amount)
    {
        HP -= amount;
        Debug.Log("Cow HP: " + HP);

        if (model != null)
            StartCoroutine(FlashRed());

        if (HP <= 0)
        {
            Destroy(gameObject);
            if (gamemanager.instance != null)
                gamemanager.instance.youLose();
        }
    }

    private IEnumerator FlashRed()
    {
        model.material.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        model.material.color = originalColor;
    }
}
