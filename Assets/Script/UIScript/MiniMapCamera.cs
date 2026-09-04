using UnityEngine;

public class MiniMapCamera : MonoBehaviour
{
    private Camera cam; 
    public float smoothSpeed = 5f;    // seberapa smooth pergerakan kamera  
    public GameObject player;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // LateUpdate is called once per frame, after Update
    void LateUpdate()
    {
        Vector3 targetPosition = new Vector3(player.transform.position.x, player.transform.position.y, -10);
        transform.position = Vector3.Lerp(transform.position, targetPosition, smoothSpeed * Time.deltaTime);
    }
}
