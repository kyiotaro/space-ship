using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    private AudioManager audioManager;
    public GameObject PlayerStats;
    public GameObject GamePlayUI;
    public GameObject pauseMenu;
    void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            bool showStats = !PlayerStats.activeSelf;
            PlayerStats.SetActive(showStats);

            if (GamePlayUI != null)
                GamePlayUI.SetActive(!showStats);

            Time.timeScale = showStats ? 0 : 1;

        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
        }
    }
    public void onResumeButtonClicked()
    {
        audioManager?.playClickSFX();
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }
    public void onRestartButtonClicked()
    {
        audioManager?.playClickSFX();
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        pauseMenu.SetActive(false);
    }
    public void onHomeButtonClicked()
    {
        audioManager?.playClickSFX();
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
        pauseMenu.SetActive(false);
    }
}
