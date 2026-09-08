using UnityEngine;

public class ButtonTest : MonoBehaviour
{
    public void ButtonClick()
    {
        PlayerStats playerStats = PlayerStats.Instance;
        LevelSystem levelSystem = LevelSystem.instance;

        if (playerStats == null)
        {
            Debug.LogError("[ButtonTest] PlayerStats.Instance is null.", this);
            return;
        }

        if (levelSystem == null)
        {
            Debug.LogError("[ButtonTest] LevelSystem.instance is null.", this);
            return;
        }

        float oldAttack = playerStats.Attack;
        int oldPoints = levelSystem.GetUpgradePoints();
        playerStats.UpgradeAttack();

        Debug.Log(
            $"[ButtonTest] UpgradeAttack clicked. Points: {oldPoints} -> {levelSystem.GetUpgradePoints()}, " +
            $"Attack: {oldAttack} -> {playerStats.Attack}.",
            this);
    }
}
