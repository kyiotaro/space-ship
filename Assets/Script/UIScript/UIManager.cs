using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UIElements;
using System;

public class UIManager : MonoBehaviour
{
    private AudioManager audioManager;
    private UIDocument uiDocument;
    private VisualElement hud;
    private VisualElement shopOverlay;
    private VisualElement statsOverlay;
    private VisualElement pauseOverlay;
    private VisualElement gameOverOverlay;
    private VisualElement shopCards;
    private Image minimapImage;
    private PlayerStats playerStats;
    private GameOver gameOver;
    private bool isStatsOpen;
    private bool isShopOpen;
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
        shopOverlay = RequireElement<VisualElement>(root, "shop-overlay");
        statsOverlay = RequireElement<VisualElement>(root, "stats-overlay");
        pauseOverlay = RequireElement<VisualElement>(root, "pause-overlay");
        gameOverOverlay = RequireElement<VisualElement>(root, "game-over-overlay");
        shopCards = RequireElement<VisualElement>(root, "shop-cards");
        minimapImage = RequireElement<Image>(root, "minimap-image");

        shopOverlay?.BringToFront();
        statsOverlay?.BringToFront();
        pauseOverlay?.BringToFront();
        gameOverOverlay?.BringToFront();

        BuildShopCards();
        RegisterButton(root, "open-shop-button", OpenShop);
        RegisterButton(root, "close-shop-button", CloseShop);
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
        SetDisplay(shopOverlay, false);
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

        if (Input.GetKeyDown(KeyCode.Tab) && !isPaused && !isShopOpen)
        {
            if (isStatsOpen)
                CloseStats();
            else
                OpenStats();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isShopOpen)
                CloseShop();
            else if (isStatsOpen)
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
        UnregisterButton(root, "open-shop-button", OpenShop);
        UnregisterButton(root, "close-shop-button", CloseShop);
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
        gameOverOverlay?.BringToFront();
        SetDisplay(gameOverOverlay, visible);
        if (visible)
        {
            isStatsOpen = false;
            isShopOpen = false;
            isPaused = false;
            SetDisplay(shopOverlay, false);
            SetDisplay(statsOverlay, false);
            SetDisplay(pauseOverlay, false);
            SetDisplay(hud, false);
            Time.timeScale = 0f;
        }
        else
        {
            SetDisplay(hud, true);
            SetDisplay(shopOverlay, false);
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
        SetDisplay(hud, true);
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
        if (isPaused || (gameOver != null && gameOver.IsSetup))
            return;

        audioManager?.playClickSFX();
        isPaused = true;
        isStatsOpen = false;
        isShopOpen = false;
        SetDisplay(hud, false);
        SetDisplay(statsOverlay, false);
        SetDisplay(shopOverlay, false);
        pauseOverlay?.BringToFront();
        SetDisplay(pauseOverlay, true);
        Time.timeScale = 0f;
    }

    private void OpenStats()
    {
        audioManager?.playClickSFX();
        isStatsOpen = true;
        statsOverlay?.BringToFront();
        SetDisplay(hud, false);
        SetDisplay(statsOverlay, true);
        Time.timeScale = 0f;
    }

    private void OpenShop()
    {
        audioManager?.playClickSFX();
        isShopOpen = true;
        shopOverlay?.BringToFront();
        SetDisplay(hud, false);
        SetDisplay(shopOverlay, true);
        Time.timeScale = 0f;
    }

    private void BuildShopCards()
    {
        if (shopCards == null)
            return;

        ShopUpgradeData[] upgrades = Resources.LoadAll<ShopUpgradeData>("ShopUpgrades");
        if (upgrades.Length == 0)
        {
            Debug.LogError("[UIManager] No shop upgrade assets were found in Resources/ShopUpgrades.", this);
            return;
        }

        Array.Sort(upgrades, (left, right) =>
        {
            if (left == null)
                return right == null ? 0 : 1;
            if (right == null)
                return -1;

            return left.SortOrder.CompareTo(right.SortOrder);
        });
        foreach (ShopUpgradeData upgrade in upgrades)
        {
            if (upgrade == null)
            {
                Debug.LogError("[UIManager] A shop upgrade asset is null.", this);
                continue;
            }

            shopCards.Add(CreateShopCard(upgrade));
        }
    }

    private static VisualElement CreateShopCard(ShopUpgradeData upgrade)
    {
        VisualElement card = new VisualElement();
        card.AddToClassList("shop-card");

        Label category = new Label(upgrade.Category);
        category.AddToClassList("shop-card-category");
        card.Add(category);

        Label title = new Label(upgrade.DisplayName);
        title.AddToClassList("shop-card-title");
        card.Add(title);

        Label description = new Label(upgrade.Description);
        description.AddToClassList("shop-card-description");
        card.Add(description);

        Label bonus = new Label(upgrade.BonusText);
        bonus.AddToClassList("shop-card-bonus");
        card.Add(bonus);

        VisualElement footer = new VisualElement();
        footer.AddToClassList("shop-card-footer");

        Label price = new Label($"{upgrade.CreditCost} CREDITS");
        price.AddToClassList("shop-card-price");
        footer.Add(price);

        Button buyButton = new Button { text = "BUY" };
        buyButton.SetEnabled(false);
        buyButton.AddToClassList("shop-buy-button");
        footer.Add(buyButton);

        card.Add(footer);
        return card;
    }

    private void CloseShop()
    {
        audioManager?.playClickSFX();
        isShopOpen = false;
        SetDisplay(shopOverlay, false);
        SetDisplay(hud, true);
        Time.timeScale = 1f;
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
