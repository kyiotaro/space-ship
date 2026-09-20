using System.Collections;
using UnityEngine;

public class PlayerCamera : MonoBehaviour
{
    public float leadStrength = 0.3f; // seberapa jauh kamera geser ke cursor (0-1)
    public float smoothSpeed = 5f;    // seberapa smooth pergerakan kamera
    public float shakeDuration = 0.15f;
    public float shakeStrength = 0.12f;
    private Camera cam;               // referensi kamera
    public GameObject player;
    private Vector3 shakeOffset;
    private Coroutine shakeRoutine;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        cam = GetComponent<Camera>();
    }

    // Update is called once per frame
    void LateUpdate()
    {
        if (cam == null)
            cam = GetComponent<Camera>();

        if (player == null)
        {
            enabled = false;
            return;
        }

        Vector3 mousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;

        Vector3 targetPos = Vector3.Lerp(player.transform.position, mousePos, leadStrength);
        targetPos.z = -10f;

        transform.position = Vector3.Lerp(transform.position, targetPos, smoothSpeed * Time.deltaTime) + shakeOffset;
    }

    public void Shake()
    {
        if (shakeRoutine != null)
            StopCoroutine(shakeRoutine);

        shakeRoutine = StartCoroutine(ShakeRoutine());
    }

    private IEnumerator ShakeRoutine()
    {
        float elapsed = 0f;

        while (elapsed < shakeDuration)
        {
            shakeOffset = Random.insideUnitCircle * shakeStrength;
            elapsed += Time.deltaTime;
            yield return null;
        }

        shakeOffset = Vector3.zero;
        shakeRoutine = null;
    }
}
