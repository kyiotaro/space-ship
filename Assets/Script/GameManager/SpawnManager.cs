using UnityEngine; 


public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject[] rareEnemyPrefab;
    public GameObject CameraPos;
    public Camera minimapCamera;
    private int enemyCount;
    private int maxEnemy;
    private int rareEnemySpawnedAtLevel = -1;

    private float spawnTimer = 0f;
    private float spawnInterval = 5f;
    [SerializeField] private float spawnMargin = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (LevelSystem.instance != null)
        {
            LevelSystem.instance.OnLevelUp += HandleLevelUp;
        }

        maxEnemy = 1;
        Debug.Log("Max Enemy: " + maxEnemy);
        SpawnEnemy();

        if (LevelSystem.instance != null)
        {
            HandleLevelUp(LevelSystem.instance.GetCurrentLevel());
        }
    }

    private void OnDestroy()
    {
        if (LevelSystem.instance != null)
        {
            LevelSystem.instance.OnLevelUp -= HandleLevelUp;
        }
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
        if (enemyPrefab == null || enemyPrefab.Length == 0) return;

        int enemyIndex = Random.Range(0, enemyPrefab.Length);
        SpawnPrefab(enemyPrefab[enemyIndex]);
    }

    private void HandleLevelUp(int newLevel)
    {
        if (rareEnemyPrefab == null || rareEnemyPrefab.Length == 0 || newLevel < 3 || newLevel % 3 != 0)
        {
            return;
        }

        if (rareEnemySpawnedAtLevel == newLevel)
        {
            return;
        }

        rareEnemySpawnedAtLevel = newLevel;
        int rareEnemyIndex = Random.Range(0, rareEnemyPrefab.Length);
        SpawnPrefab(rareEnemyPrefab[rareEnemyIndex]);
    }

    private void SpawnPrefab(GameObject prefab)
    {
        if (prefab == null) return;

        Vector3 spawnPosition = transform.position;

        if (minimapCamera != null && minimapCamera.orthographic)
        {
            float visibleHeight = minimapCamera.orthographicSize * 2f;
            float visibleWidth = visibleHeight * minimapCamera.aspect;

            spawnPosition.x = Random.Range(
                minimapCamera.transform.position.x - visibleWidth / 2f,
                minimapCamera.transform.position.x + visibleWidth / 2f);
            spawnPosition.y = minimapCamera.transform.position.y + visibleHeight / 2f + spawnMargin;
        }

        Instantiate(prefab, spawnPosition, prefab.transform.rotation);
    }
}
