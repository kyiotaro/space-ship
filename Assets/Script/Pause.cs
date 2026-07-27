using UnityEngine;

public class Pause : MonoBehaviour
{
    private bool isPaused;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        isPaused = false;
    }

    // Update is called once per frame
    void Update()
    {
       if(Input.GetKeyDown(KeyCode.Escape))
        {
            if(isPaused)
            {
                resume();
            }
            else
            {
                pause();
            }
        } 
    }

    void pause()
    {
        Time.timeScale = 0;
        isPaused = true;
    }

    void resume()
    {
        Time.timeScale = 1;
        isPaused = false;
    }
}

