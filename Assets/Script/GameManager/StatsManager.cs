using UnityEngine;
using TMPro;

public class StatsManager : MonoBehaviour
{
    private
     float Health;
    public float maxHealth;
    public float Attack;
    public float Defense;
    public float Speed;

    public TextMeshProUGUI HealthText;
    public TextMeshProUGUI AttackText;
    public TextMeshProUGUI DefenseText;
    public TextMeshProUGUI SpeedText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Health = maxHealth;
    }

    // Update is called once per frame
    void Update()
    {
        HealthText.text = "Health: " + Health.ToString() + "/" + maxHealth.ToString();
        AttackText.text = "Attack: " + Attack.ToString();
        DefenseText.text = "Defense: " + Defense.ToString();
        SpeedText.text = "Speed: " + Speed.ToString();

    }
}
