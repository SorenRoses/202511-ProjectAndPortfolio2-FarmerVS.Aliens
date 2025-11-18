using System.Collections;
using UnityEngine;

public class waveManager : MonoBehaviour
{
    public static waveManager instance;

    [SerializeField] spawner [] waveSpawner;
    [SerializeField] float [] delayBeforEachWave;
    [SerializeField] float timeBetweenWaves;
    [SerializeField] bool autoStartFirstWave = true;

    int currentWave = 0;
    bool waveActive = false;
    private bool advancingToNextWave = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        instance = this;
    }
    void Start()
    {
        //gamemanager.instance.updateGameGoal(0);
        gamemanager.instance.SetCurrentWave(-1);

        if (autoStartFirstWave && waveSpawner != null && waveSpawner.Length > 0)
        {
            StartCoroutine(StartWave(0));
        }

    }

    // Update is called once per frame
    void Update()
    {
        if (waveActive && gamemanager.instance.gameGoalCount <= 0)
        {
            advancingToNextWave = true;
            StartCoroutine(GoToNextWave());
        }
    }

    IEnumerator StartWave(int waveIndex)
    {
        currentWave = waveIndex;
        waveActive = true;

        yield return new WaitForSeconds(delayBeforEachWave[waveIndex]);

        for (int i = 0; i < waveSpawner.Length; i++)
            waveSpawner[i].StopSpawning();

        waveSpawner[waveIndex].StartSpawning();

        gamemanager.instance.SetCurrentWave(waveIndex);
    }

    IEnumerator GoToNextWave()
    {
        waveActive = false;
        yield return new WaitForSeconds(timeBetweenWaves);

        int next = currentWave + 1;
        if (next < waveSpawner.Length)
        {
            advancingToNextWave = false;
            StartCoroutine(StartWave(next));
        }
        else
        {
            advancingToNextWave = false;
        }
    }
  
    public void StartFirstWave()
    {
        StopAllCoroutines();
        StartCoroutine(StartWave(0));
    }
  
    
}
