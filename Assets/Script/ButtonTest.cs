using UnityEngine;

public class ButtonTest : MonoBehaviour
{
    [SerializeField] private float attackUpgradeAmount = 10f;

    private AudioManager audioManager;

    private void Awake()
    {
        audioManager = FindFirstObjectByType<AudioManager>();
    }

    public void ButtonClick()
    {
        audioManager?.playClickSFX();
        PlayerStats playerStats = PlayerStats.Instance;

        if (playerStats == null)
        {
            Debug.LogError("[ButtonTest] PlayerStats.Instance is null.", this);
            return;
        }

        Point pointManager = Point.instance;
        if (pointManager == null)
        {
            Debug.LogError("[ButtonTest] Point.instance is null.", this);
            return;
        }

        float oldAttack = playerStats.Attack;
        int oldPoints = pointManager.Balance;
        playerStats.UpgradeAttack(attackUpgradeAmount);

        Debug.Log(
            $"[ButtonTest] UpgradeAttack clicked. Points: {oldPoints} -> {pointManager.Balance}, " +
            $"Attack: {oldAttack} -> {playerStats.Attack}.",
            this);
    }
}
