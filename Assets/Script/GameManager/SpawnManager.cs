using UnityEngine; 


public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject CameraPos;
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
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        transform.position = new Vector3(CameraPos.transform.position.x, CameraPos.transform.position.y + 10, transform.position.z);
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && enemyCount < maxEnemy)
        {
            spawnTimer = 0f;           
            SpawnEnemy();
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
