using System.Collections.Generic;
using UnityEngine;


public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject[] rareEnemyPrefab;
    public GameObject CameraPos;
    public Camera minimapCamera;

    private const int InitialEnemiesPerWave = 10;
    private const int AdditionalEnemiesPerWave = 5;

    private readonly List<GameObject> spawnedEnemies = new List<GameObject>();
    private int currentWave = 1;
    private int enemiesSpawnedThisWave;
    private float spawnTimer = 0f;
    private float spawnInterval = 5f;
    [SerializeField] private float spawnMargin = 2f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (!HasPrefab(enemyPrefab))
            Debug.LogError("[SpawnManager] No valid enemy prefabs are assigned.", this);

        StartNextWave();
    }

    // Update is called once per frame
    void Update()
    {
        if (CameraPos == null)
        {
            enabled = false;
            return;
        }

        transform.position = new Vector3(CameraPos.transform.position.x, CameraPos.transform.position.y + 10, transform.position.z);

        spawnedEnemies.RemoveAll(enemy => enemy == null);
        if (enemiesSpawnedThisWave >= GetEnemiesPerWave())
        {
            if (spawnedEnemies.Count == 0)
            {
                currentWave++;
                StartNextWave();
            }

            return;
        }

        spawnTimer += Time.deltaTime;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            if (SpawnEnemy())
                enemiesSpawnedThisWave++;
        }
    }

    private void StartNextWave()
    {
        enemiesSpawnedThisWave = 0;
        spawnTimer = 0f;
        Debug.Log($"Starting wave {currentWave}: {GetEnemiesPerWave()} enemies.");

        if (SpawnEnemy())
            enemiesSpawnedThisWave++;
    }

    private int GetEnemiesPerWave()
    {
        return InitialEnemiesPerWave + (currentWave - 1) * AdditionalEnemiesPerWave;
    }

    private bool SpawnEnemy()
    {
        if (currentWave % 3 == 0 && enemiesSpawnedThisWave == 0 && TrySpawnFromPrefabs(rareEnemyPrefab))
            return true;

        return TrySpawnFromPrefabs(enemyPrefab);
    }

    private bool TrySpawnFromPrefabs(GameObject[] prefabs)
    {
        if (prefabs == null || prefabs.Length == 0)
            return false;

        int startIndex = Random.Range(0, prefabs.Length);
        for (int i = 0; i < prefabs.Length; i++)
        {
            GameObject prefab = prefabs[(startIndex + i) % prefabs.Length];
            if (prefab == null)
                continue;

            GameObject spawnedEnemy = SpawnPrefab(prefab);
            if (spawnedEnemy != null)
            {
                spawnedEnemies.Add(spawnedEnemy);
                return true;
            }
        }

        return false;
    }

    private bool HasPrefab(GameObject[] prefabs)
    {
        if (prefabs == null)
            return false;

        foreach (GameObject prefab in prefabs)
        {
            if (prefab != null)
                return true;
        }

        return false;
    }

    private GameObject SpawnPrefab(GameObject prefab)
    {
        if (prefab == null) return null;

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

        return Instantiate(prefab, spawnPosition, prefab.transform.rotation);
    }
}
