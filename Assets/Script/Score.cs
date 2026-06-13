using UnityEngine;
using UnityEngine.UI;

public class Score : MonoBehaviour
{   
    public static Score instance;
    public Text scoreText;
    public Text highScoreText;

    private int score = 0;
    private int highScore = 0;

     private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        highScore = PlayerPrefs.GetInt("highScore", 0);
        scoreText.text = "SCORES: " + score.ToString();
        highScoreText.text = "HIGH SCORES: " + highScore.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int points)
    {
        score += points;
        scoreText.text = "SCORES: " + score.ToString();

        if(score > highScore)
        {
            PlayerPrefs.SetInt("highScore", score);
        }
    }
}
