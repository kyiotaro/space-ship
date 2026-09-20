using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private AudioManager audioManager;
    private bool isSetup;

    void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        isSetup = false;
    }

    void Update()
    {
        if (isSetup && Input.GetMouseButtonDown(0))
        {
            restart(); 
        }
    }

    public void setup(bool value)
    {
        Debug.Log($"[GameOver] setup({value}) called. Panel before: {gameObject.activeSelf}", this);
        gameObject.SetActive(value); 
        isSetup = value;
        Debug.Log($"[GameOver] Panel after: {gameObject.activeSelf}, isSetup: {isSetup}", this);
    }
    public void restart()
    {
        audioManager?.playClickSFX();
        SceneManager.LoadScene("Game");
    }
}
