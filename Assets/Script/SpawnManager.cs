using UnityEngine; 


public class SpawnManager : MonoBehaviour
{
    public GameObject[] enemyPrefab;
    public GameObject CameraPos;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(CameraPos.transform.position.x, CameraPos.transform.position.y + 10f, 0f);
        if (Input.GetKeyDown(KeyCode.E))
        {
            SpawnEnemy();
        }
    }

    void SpawnEnemy()
    {
        int enemyIndex = Random.Range(0, enemyPrefab.Length);
        Instantiate(enemyPrefab[enemyIndex], transform.position, enemyPrefab[enemyIndex].transform.rotation);
    }
}
