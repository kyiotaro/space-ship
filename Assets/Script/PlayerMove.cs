using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f; // Player movement speed
    public float rotationSpeed = 45f; // Player rotation speed
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        
        Vector3 rotation = new Vector3(0, 0, horizontalInput) * rotationSpeed * Time.deltaTime;
        Vector2 movement = new Vector2(0, verticalInput) * speed * Time.deltaTime;
        transform.Translate(movement);
        transform.Rotate(-rotation);
    }
}
