using UnityEngine;
using UnityEngine.SceneManagement;

public class GameOver : MonoBehaviour
{
    private bool isSetup;

    void Start()
    {   
        isSetup = false;
        setup(false);
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
        gameObject.SetActive(value); 
        isSetup = value;
    }
    public void restart()
    {
        SceneManager.LoadScene("Game");
    }
}
