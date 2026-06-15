using UnityEngine; 


public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject CameraPos;
    private GameObject[] spawnedEnemies;
    private int enemyCount;
    private int maxEnemy;
    private int targetScore;

    private float spawnTimer = 0f;
    private float spawnInterval = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        targetScore = 30;
        maxEnemy = 1;
        Debug.Log("Max Enemy: " + maxEnemy);
        spawnedEnemies = GameObject.FindGameObjectsWithTag("Enemy");
        enemyCount = spawnedEnemies.Length;
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && enemyCount < maxEnemy)
        {
            spawnTimer = 0f;
            SpawnEnemy();
            Debug.Log("Enemy count: " + enemyCount);
        }

        if (Score.instance.score == targetScore && Score.instance.score > 0)
        {
            maxEnemy += 1;
            Debug.Log("Max Enemy: " + maxEnemy);   
            targetScore += 30;
        }

        
    }

    void SpawnEnemy()
    {
        int enemyIndex = Random.Range(0, enemyPrefab.Length);
        Instantiate(enemyPrefab[enemyIndex], transform.position, enemyPrefab[enemyIndex].transform.rotation);
    }
}
