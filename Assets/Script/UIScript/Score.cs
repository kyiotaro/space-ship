using UnityEngine;
using TMPro;

public class Score : MonoBehaviour
{   
    public static Score instance;
    public TMP_Text scoreText;
    public TMP_Text highScoreText;

    public int score = 0;
    private int highScore = 0;

    public static float DifficultyMultiplier => instance != null ? instance.GetDifficultyMultiplier() : 1f;

    [Header("Enemy Difficulty")]
    [SerializeField] private int scorePerDifficultyStep = 100;
    [SerializeField] private float difficultyIncreasePerStep = 0.1f;
    [SerializeField] private float maxDifficultyMultiplier = 3f;

     private void Awake()
    {
        instance = this;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        highScore = PlayerPrefs.GetInt("highScore", 0);
        UpdateScoreUI();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AddScore(int points)
    {
        if (points <= 0) return;

        score += points;
        UpdateScoreUI();

        if (score > highScore)
        {
            highScore = score;
            PlayerPrefs.SetInt("highScore", score);
            PlayerPrefs.Save();
            UpdateScoreUI();
        }
    }

    [ContextMenu("Reset High Score")]
    public void ResetHighScore()
    {
        highScore = 0;
        PlayerPrefs.DeleteKey("highScore");
        PlayerPrefs.Save();
        UpdateScoreUI();
    }

    private float GetDifficultyMultiplier()
    {
        if (scorePerDifficultyStep <= 0)
            return 1f;

        float steps = Mathf.Floor((float)score / scorePerDifficultyStep);
        return Mathf.Min(maxDifficultyMultiplier, 1f + steps * difficultyIncreasePerStep);
    }

    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "SCORES: " + score;

        if (highScoreText != null)
            highScoreText.text = "HIGH SCORES: " + highScore;
    }
}
