using UnityEngine;
using UnityEngine.UIElements;

public class Score : MonoBehaviour
{   
    public static Score instance;

    public int score = 0;
    private int highScore = 0;
    private Label scoreLabel;
    private Label highScoreLabel;
    public int HighScore => highScore;

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
        UIDocument document = FindFirstObjectByType<UIDocument>();
        if (document == null)
        {
            Debug.LogError("[Score] No UI Toolkit document was found.", this);
            return;
        }

        scoreLabel = document.rootVisualElement.Q<Label>("score-value");
        highScoreLabel = document.rootVisualElement.Q<Label>("high-score");
        if (scoreLabel == null || highScoreLabel == null)
        {
            Debug.LogError("[Score] The UI Toolkit document is missing the score or high-score label.", this);
            return;
        }

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
        if (scoreLabel != null)
            scoreLabel.text = score.ToString();
        if (highScoreLabel != null)
            highScoreLabel.text = highScore.ToString();
    }
}
