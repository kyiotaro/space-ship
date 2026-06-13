using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    public void setup()
    {
        gameObject.SetActive(true);

    }

    public void restart()
    {
        SceneManager.LoadScene("Game");
    }
}
