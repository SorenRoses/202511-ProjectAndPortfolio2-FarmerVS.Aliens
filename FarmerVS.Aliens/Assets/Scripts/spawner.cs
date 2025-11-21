using System.Collections.Generic;
using UnityEngine;

public class spawner : MonoBehaviour
{
    [SerializeField] private GameObject objectToSpawn;
    [SerializeField] public int spawnAmount;
    [SerializeField] private float spawnRate;
    [SerializeField] private Transform[] spawnPos;

    private int spawnCount;
    private float spawnTimer;
    private bool startSpawning;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    private bool finishedSpawning = false;
    public bool HasFinishedSpawning => finishedSpawning;

    public void StartSpawning()
    {
        startSpawning = true;
        finishedSpawning = false;
        spawnCount = 0;
        spawnTimer = 0;
        spawnedEnemies.Clear();
    }

    public void StopSpawning()
    {
        startSpawning = false;
        finishedSpawning = true;
        spawnedEnemies.Clear();
    }

    void Update()
    {
        if (startSpawning)
        {
            spawnTimer += Time.deltaTime;

            if (spawnCount < spawnAmount && spawnTimer >= spawnRate)
            {
                Spawn();
            }

            
            if (spawnCount >= spawnAmount)
            {
                finishedSpawning = true;
                startSpawning = false;
            }
        }

        
        spawnedEnemies.RemoveAll(e => e == null);
    }

    void Spawn()
    {
        GameObject enemy = Instantiate(objectToSpawn,
                                       spawnPos[Random.Range(0, spawnPos.Length)].position,
                                       Quaternion.identity);
        spawnedEnemies.Add(enemy);

        spawnCount++;
        spawnTimer = 0;
    }

    
    public bool AreAllEnemiesDead()
    {
        spawnedEnemies.RemoveAll(e => e == null);
        return spawnedEnemies.Count == 0;
    }
}