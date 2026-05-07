using UnityEngine;

public class FollowPlayer : MonoBehaviour
{
    public float leadStrength = 0.3f; // seberapa jauh kamera geser ke cursor (0-1)
    public float smoothSpeed = 5f;    // seberapa smooth pergerakan kamera
    private Camera cam;               // referensi kamera
    public GameObject player;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 targetPos = Vector3.Lerp(player.transform.position, mousePos, leadStrength);
        targetPos.z = -10f;

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime);
    }
}
