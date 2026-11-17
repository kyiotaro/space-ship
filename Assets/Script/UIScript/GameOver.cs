using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private AudioManager audioManager;
    private bool isSetup;
    public bool IsSetup => isSetup;

    void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        isSetup = false;
    }

    public void setup(bool value)
    {
        isSetup = value;
        UIManager uiManager = FindFirstObjectByType<UIManager>();
        if (uiManager == null)
        {
            Debug.LogError("[GameOver] UIManager was not found.", this);
            return;
        }

        uiManager.SetGameOverVisible(value);
    }
    public void restart()
    {
        audioManager?.playClickSFX();
        SceneManager.LoadScene("Game");
    }
}
