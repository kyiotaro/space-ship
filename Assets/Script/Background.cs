using System.Collections.Generic;
using UnityEngine;

public class Background : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject[] Planets;
    public GameObject[] Stars;

    [Header("Settings")]
    public int cellSize = 30;      // Diperbesar sedikit karena planet sekarang besar
    public int starDensity = 25;   // Lebih banyak bintang per cell
    public int planetDensity = 1;  // Max planet per cell
    public float zPosition = 10f;  // Jarak ke belakang

    private Camera cam;
    private HashSet<Vector2Int> generatedCells = new HashSet<Vector2Int>();

    void Start()
    {
        cam = Camera.main;
    }

    void Update()
    {
        if (cam == null) return;

        // Tentukan area yang terlihat oleh kamera (dalam world space)
        Vector3 camPos = cam.transform.position;
        float height = 2f * cam.orthographicSize;
        float width = height * cam.aspect;

        float left = camPos.x - width / 2;
        float right = camPos.x + width / 2;
        float top = camPos.y + height / 2;
        float bottom = camPos.y - height / 2;

        // Cari cell mana saja yang bersinggungan dengan view kamera
        int minX = Mathf.FloorToInt(left / cellSize);
        int maxX = Mathf.FloorToInt(right / cellSize);
        int minY = Mathf.FloorToInt(bottom / cellSize);
        int maxY = Mathf.FloorToInt(top / cellSize);

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2Int cell = new Vector2Int(x, y);
                if (!generatedCells.Contains(cell))
                {
                    GenerateCell(cell);
                    generatedCells.Add(cell);
                }
            }
        }
    }

    void GenerateCell(Vector2Int cell)
    {
        // Generate Bintang (Sangat Banyak)
        int starsToSpawn = Random.Range(starDensity / 2, starDensity);
        for (int i = 0; i < starsToSpawn; i++)
        {
            SpawnObject(Stars, cell, 0.3f, 1.0f);
        }

        // Generate Planet (Besar & Jarang)
        if (Random.value < 0.15f) // 15% chance ada planet di cell ini
        {
            int planetsToSpawn = Random.Range(0, planetDensity + 1);
            for (int i = 0; i < planetsToSpawn; i++)
            {
                SpawnObject(Planets, cell, 4.0f, 6.0f); // Scale 4-6 sesuai request
            }
        }
    }

    void SpawnObject(GameObject[] prefabs, Vector2Int cell, float minScale, float maxScale)
    {
        if (prefabs == null || prefabs.Length == 0) return;

        GameObject prefab = prefabs[Random.Range(0, prefabs.Length)];
        
        // Random posisi di dalam cell
        float posX = Random.Range(cell.x * cellSize, (cell.x + 1) * cellSize);
        float posY = Random.Range(cell.y * cellSize, (cell.y + 1) * cellSize);
        Vector3 pos = new Vector3(posX, posY, zPosition);

        GameObject spawned = Instantiate(prefab, pos, Quaternion.identity, transform);
        
        // Random rotasi dan skala biar variatif
        spawned.transform.rotation = Quaternion.Euler(0, 0, Random.Range(0, 360));
        float scale = Random.Range(minScale, maxScale);
        spawned.transform.localScale = new Vector3(scale, scale, 1);
    }
}
