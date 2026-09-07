using UnityEngine; 


public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject CameraPos;
    private int enemyCount;
    private int maxEnemy;

    private float spawnTimer = 0f;
    private float spawnInterval = 5f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        maxEnemy = 1;
        Debug.Log("Max Enemy: " + maxEnemy);
        SpawnEnemy();
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraPos == null)
        {
            enabled = false;
            return;
        }

        enemyCount = GameObject.FindGameObjectsWithTag("Enemy").Length;
        maxEnemy = LevelSystem.instance != null
            ? Mathf.Max(1, LevelSystem.instance.GetCurrentLevel())
            : 1;

        transform.position = new Vector3(CameraPos.transform.position.x, CameraPos.transform.position.y + 10, transform.position.z);
        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval && enemyCount < maxEnemy)
        {
            spawnTimer = 0f;           
            SpawnEnemy();
        }

    }

    void SpawnEnemy()
    {
        int enemyIndex = Random.Range(0, enemyPrefab.Length);
        Instantiate(enemyPrefab[enemyIndex], transform.position, enemyPrefab[enemyIndex].transform.rotation);
    }
}
