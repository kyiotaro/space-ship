using UnityEngine;

public class UIManagerMainMenu : MonoBehaviour
{
    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    public void onPlayButtonClicked()
    {
        audioManager?.playClickSFX();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
