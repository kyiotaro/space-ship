using UnityEngine;
using UnityEngine.UIElements;

public class UIManagerMainMenu : MonoBehaviour
{
    private AudioManager audioManager;
    private Button playButton;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    private void Start()
    {
        foreach (Canvas canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))
            canvas.enabled = false;

        UIDocument document = FindFirstObjectByType<UIDocument>();
        if (document == null)
        {
            Debug.LogError("[UIManagerMainMenu] No UI Toolkit document was found.", this);
            enabled = false;
            return;
        }

        playButton = document.rootVisualElement.Q<Button>("play-button");
        if (playButton == null)
        {
            Debug.LogError("[UIManagerMainMenu] The UI Toolkit document is missing the play button.", this);
            enabled = false;
            return;
        }

        playButton.clicked += onPlayButtonClicked;
    }

    private void OnDestroy()
    {
        if (playButton != null)
            playButton.clicked -= onPlayButtonClicked;
    }

    public void onPlayButtonClicked()
    {
        audioManager?.playClickSFX();
        UnityEngine.SceneManagement.SceneManager.LoadScene("Game");
    }
}
