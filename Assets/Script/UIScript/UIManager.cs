using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{
    private AudioManager audioManager;
    private UIDocument uiDocument;
    private VisualElement hud;
    private VisualElement statsOverlay;
    private VisualElement pauseOverlay;
    private VisualElement gameOverOverlay;
    private Image minimapImage;
    private PlayerStats playerStats;
    private GameOver gameOver;
    private bool isStatsOpen;
    private bool isPaused;

    private void Start()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
        uiDocument = FindFirstObjectByType<UIDocument>();
        if (uiDocument == null)
        {
            Debug.LogError("[UIManager] No UI Toolkit document was found.", this);
            enabled = false;
            return;
        }

        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            canvas.enabled = false;

        VisualElement root = uiDocument.rootVisualElement;
        hud = RequireElement<VisualElement>(root, "hud");
        statsOverlay = RequireElement<VisualElement>(root, "stats-overlay");
        pauseOverlay = RequireElement<VisualElement>(root, "pause-overlay");
        gameOverOverlay = RequireElement<VisualElement>(root, "game-over-overlay");
        minimapImage = RequireElement<Image>(root, "minimap-image");

        RegisterButton(root, "pause-button", Pause);
        RegisterButton(root, "resume-button", onResumeButtonClicked);
        RegisterButton(root, "restart-button", onRestartButtonClicked);
        RegisterButton(root, "home-button", onHomeButtonClicked);
        RegisterButton(root, "close-stats-button", CloseStats);
        RegisterButton(root, "upgrade-attack", UpgradeAttack);
        RegisterButton(root, "upgrade-defense", UpgradeDefense);
        RegisterButton(root, "upgrade-speed", UpgradeSpeed);
        RegisterButton(root, "upgrade-health", UpgradeHealth);

        gameOver = FindFirstObjectByType<GameOver>();
        if (gameOver == null)
            Debug.LogError("[UIManager] GameOver component was not found.", this);
        else
            RegisterButton(root, "game-over-restart-button", gameOver.restart);

        playerStats = PlayerStats.Instance;
        if (playerStats == null)
            Debug.LogError("[UIManager] PlayerStats was not found.", this);

        Camera minimapCamera = GameObject.Find("MiniMap Camera")?.GetComponent<Camera>();
        if (minimapImage == null || minimapCamera == null || minimapCamera.targetTexture == null)
            Debug.LogError("[UIManager] The minimap camera or its render texture is missing.", this);
        else
            minimapImage.image = minimapCamera.targetTexture;

        SetDisplay(statsOverlay, false);
        SetDisplay(pauseOverlay, false);
        if (gameOver != null && gameOver.IsSetup)
            SetGameOverVisible(true);
        else
        {
            SetDisplay(gameOverOverlay, false);
            Time.timeScale = 1f;
        }
    }

    private void Update()
    {
        if (gameOver != null && gameOver.IsSetup)
            return;

        if (Input.GetKeyDown(KeyCode.Tab) && !isPaused)
        {
            if (isStatsOpen)
                CloseStats();
            else
                OpenStats();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isStatsOpen)
                CloseStats();
            else if (isPaused)
                onResumeButtonClicked();
            else
                Pause();
        }
    }

    private void OnDestroy()
    {
        if (uiDocument == null)
            return;

        VisualElement root = uiDocument.rootVisualElement;
        UnregisterButton(root, "pause-button", Pause);
        UnregisterButton(root, "resume-button", onResumeButtonClicked);
        UnregisterButton(root, "restart-button", onRestartButtonClicked);
        UnregisterButton(root, "home-button", onHomeButtonClicked);
        UnregisterButton(root, "close-stats-button", CloseStats);
        UnregisterButton(root, "upgrade-attack", UpgradeAttack);
        UnregisterButton(root, "upgrade-defense", UpgradeDefense);
        UnregisterButton(root, "upgrade-speed", UpgradeSpeed);
        UnregisterButton(root, "upgrade-health", UpgradeHealth);
        if (gameOver != null)
            UnregisterButton(root, "game-over-restart-button", gameOver.restart);
    }

    public void SetGameOverVisible(bool visible)
    {
        SetDisplay(gameOverOverlay, visible);
        if (visible)
        {
            isStatsOpen = false;
            isPaused = false;
            SetDisplay(statsOverlay, false);
            SetDisplay(pauseOverlay, false);
            SetDisplay(hud, false);
            Time.timeScale = 0f;
        }
        else
        {
            SetDisplay(hud, true);
            SetDisplay(statsOverlay, false);
            SetDisplay(pauseOverlay, false);
            Time.timeScale = 1f;
        }
    }

    public void onResumeButtonClicked()
    {
        audioManager?.playClickSFX();
        isPaused = false;
        SetDisplay(pauseOverlay, false);
        Time.timeScale = 1f;
    }

    public void onRestartButtonClicked()
    {
        audioManager?.playClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Game");
    }

    public void onHomeButtonClicked()
    {
        audioManager?.playClickSFX();
        Time.timeScale = 1f;
        SceneManager.LoadScene("Home");
    }

    private void Pause()
    {
        audioManager?.playClickSFX();
        isPaused = true;
        isStatsOpen = false;
        SetDisplay(statsOverlay, false);
        SetDisplay(pauseOverlay, true);
        Time.timeScale = 0f;
    }

    private void OpenStats()
    {
        audioManager?.playClickSFX();
        isStatsOpen = true;
        SetDisplay(hud, false);
        SetDisplay(statsOverlay, true);
        Time.timeScale = 0f;
    }

    private void CloseStats()
    {
        audioManager?.playClickSFX();
        isStatsOpen = false;
        SetDisplay(statsOverlay, false);
        SetDisplay(hud, true);
        Time.timeScale = 1f;
    }

    private void UpgradeAttack()
    {
        audioManager?.playClickSFX();
        playerStats?.UpgradeAttack(10f);
    }

    private void UpgradeDefense()
    {
        audioManager?.playClickSFX();
        playerStats?.UpgradeDefense(1f);
    }

    private void UpgradeSpeed()
    {
        audioManager?.playClickSFX();
        playerStats?.UpgradeTopSpeed(1f);
    }

    private void UpgradeHealth()
    {
        audioManager?.playClickSFX();
        playerStats?.UpgradeMaxHealth(25f);
    }

    private T RequireElement<T>(VisualElement root, string elementName) where T : VisualElement
    {
        T element = root.Q<T>(elementName);
        if (element == null)
            Debug.LogError($"[UIManager] UI Toolkit element '{elementName}' was not found.", this);

        return element;
    }

    private void RegisterButton(VisualElement root, string elementName, System.Action action)
    {
        Button button = RequireElement<Button>(root, elementName);
        if (button != null)
            button.clicked += action;
    }

    private void UnregisterButton(VisualElement root, string elementName, System.Action action)
    {
        Button button = root.Q<Button>(elementName);
        if (button != null)
            button.clicked -= action;
    }

    private static void SetDisplay(VisualElement element, bool visible)
    {
        if (element != null)
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }
}
