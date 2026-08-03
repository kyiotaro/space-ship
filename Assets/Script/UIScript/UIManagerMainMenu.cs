using UnityEngine;

public class UIManagerMainMenu : MonoBehaviour
{
    public void onPlayButtonClicked()
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
