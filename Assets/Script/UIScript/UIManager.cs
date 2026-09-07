using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public GameObject PlayerStats;
    public GameObject pauseMenu;
    void Start()
    {
        Time.timeScale = 1;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            if (PlayerStats.activeSelf)
            {
                PlayerStats.SetActive(false);
                Time.timeScale = 1;
            }
            else
            {
                PlayerStats.SetActive(true);
                Time.timeScale = 0;
            }

        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0;
            pauseMenu.SetActive(true);
        }
    }
    public void onResumeButtonClicked()
    {
        Time.timeScale = 1;
        pauseMenu.SetActive(false);
    }
    public void onRestartButtonClicked()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
        pauseMenu.SetActive(false);
    }
    public void onHomeButtonClicked()
    {
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Home");
        pauseMenu.SetActive(false);
    }
}
