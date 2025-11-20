using System.Collections;
using UnityEngine;

public class waveManager : MonoBehaviour
{
    public static waveManager instance;

    [SerializeField] spawner [] waveSpawner;
    [SerializeField] float [] delayBeforeEachWave;
    [SerializeField] float timeBetweenWaves;
    [SerializeField] bool autoStartFirstWave = true;

    [SerializeField] string enemyTag = "Enemy";

    public int currentWave = 0;
    bool waveActive = false;
    private bool isTransitioning= false;

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
        if (!waveActive || isTransitioning)
            return;
        if (currentWave < 0 || currentWave >= waveSpawner.Length)
            return;
       

        spawner currentSpawner = waveSpawner[currentWave];

        if (currentSpawner.HasFinishedSpawning && !AnyEnemiesAlive())
        
        {
            isTransitioning = true;
            StartCoroutine(GoToNextWave());
        }
    }

    public bool AnyEnemiesAlive()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag(enemyTag);
        return enemies.Length > 0;
       
    }



    IEnumerator StartWave(int waveIndex)
    {
        currentWave = waveIndex;
        waveActive = true;
        isTransitioning = false;

        gamemanager.instance.SetCurrentWave(waveIndex); 

        yield return new WaitForSeconds(delayBeforeEachWave[waveIndex]);

        for (int i = 0; i < waveSpawner.Length; i++)
            waveSpawner[i].StopSpawning();

        waveSpawner[waveIndex].StartSpawning();

    }

    IEnumerator GoToNextWave()
    {
        yield return new WaitForSeconds(timeBetweenWaves);

        int next = currentWave + 1;
        if (next < waveSpawner.Length)
        {
            StartCoroutine(StartWave(next));
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
  
    
}
