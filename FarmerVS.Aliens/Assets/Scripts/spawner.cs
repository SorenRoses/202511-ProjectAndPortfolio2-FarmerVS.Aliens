using UnityEngine;

public class spawner : MonoBehaviour
{
    [SerializeField] GameObject objectToSpawn;
    [SerializeField] public int spawnAmount;
    [SerializeField] int spawnRate; 
    [SerializeField] Transform[] spawnPos;

    int spawnCount;
    float spawnTimer;
    bool startSpawning;

    
    public void StartSpawning()
    {
        startSpawning = true;
        spawnCount = 0;
        spawnTimer = 0;

        
            gamemanager.instance.updateGameGoal(spawnAmount);
    }

    public void StopSpawning()
    {
        startSpawning = false;
    }

    void Update()
    {
        if (startSpawning)
        {
            spawnTimer += Time.deltaTime;
            if (spawnCount < spawnAmount && spawnTimer >= spawnRate)
            {
                spawn();
            }
        }
    }

   
  
    void spawn()
    {
        Instantiate(objectToSpawn,
                    spawnPos[Random.Range(0, spawnPos.Length)].position,
                    Quaternion.identity);
        spawnCount++;
        spawnTimer = 0;
        
    }
}