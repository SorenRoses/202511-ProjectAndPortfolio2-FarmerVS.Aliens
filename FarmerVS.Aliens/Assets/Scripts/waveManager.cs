using System.Collections;
using UnityEngine;

public class waveManager : MonoBehaviour
{
    public static waveManager instance;

    [Header("Wave Settings")]
    [SerializeField] private spawner[] waveSpawner;
    [SerializeField] private float[] delayBeforeEachWave;
    [SerializeField] private float timeBetweenWaves;
    [SerializeField] private bool autoStartFirstWave = true;

    [Header("Enemy Settings")]
    [SerializeField] private string enemyTag = "Enemy";

    [Header("Wave State")]
    public int currentWave = 0;
    private bool waveActive = false;
    private bool isTransitioning = false;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        gamemanager.instance.SetCurrentWave(-1);

        if (autoStartFirstWave && waveSpawner != null && waveSpawner.Length > 0)
        {
            StartCoroutine(StartWave(0));
        }
    }

    private void Update()
    {
        if (!waveActive || isTransitioning)
            return;

        if (currentWave < 0 || currentWave >= waveSpawner.Length)
            return;

        spawner currentSpawner = waveSpawner[currentWave];

        
        if (currentSpawner.HasFinishedSpawning && currentSpawner.AreAllEnemiesDead())
        {
            isTransitioning = true;
            StartCoroutine(GoToNextWave());
        }
    }

    private IEnumerator StartWave(int waveIndex)
    {
        currentWave = waveIndex;
        waveActive = true;
        isTransitioning = false;

        gamemanager.instance.SetCurrentWave(waveIndex);

        if (delayBeforeEachWave.Length > waveIndex)
            yield return new WaitForSeconds(delayBeforeEachWave[waveIndex]);

        
        for (int i = 0; i < waveSpawner.Length; i++)
            waveSpawner[i].StopSpawning();

        waveSpawner[waveIndex].StartSpawning();
    }

    private IEnumerator GoToNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        int nextWave = currentWave + 1;
        if (nextWave < waveSpawner.Length)
        {
            StartCoroutine(StartWave(nextWave));
        }
        else
        {
            waveActive = false;
            isTransitioning = false;
            gamemanager.instance?.CheckForWin();
        }
    }

    public void StartFirstWave()
    {
        StopAllCoroutines();
        isTransitioning = false;
        StartCoroutine(StartWave(0));
    }

    public bool AnyEnemiesAlive()
    {
        
        foreach (var s in waveSpawner)
        {
            if (s != null && !s.AreAllEnemiesDead())
                return true;
        }
        return false;
    }
}