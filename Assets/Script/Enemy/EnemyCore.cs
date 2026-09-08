using UnityEngine;

public class EnemyCore : MonoBehaviour
{
    [Header("Target")]
    [SerializeField] private GameObject target;
    [SerializeField] private string targetTag = "Player";

    [Header("Behavior")]
    public bool IsMovementEnabled = true;
    public bool IsShootingEnabled = true;
    [SerializeField] private bool pauseWhenInvisible = true;

    private Renderer enemyRenderer;

    public Transform Target => target != null ? target.transform : null;
    public bool CanMove => IsMovementEnabled && HasTarget;
    public bool CanShoot => IsShootingEnabled && HasTarget && IsVisibleOrUnrestricted;
    public bool HasTarget => target != null;

    private bool IsVisibleOrUnrestricted => !pauseWhenInvisible || enemyRenderer == null || enemyRenderer.isVisible;

    private void Awake()
    {
        enemyRenderer = GetComponent<Renderer>();
    }

    private void Start()
    {
        FindTargetIfNeeded();
    }

    private void Update()
    {
        FindTargetIfNeeded();
    }

    public void SetTarget(GameObject newTarget)
    {
        target = newTarget;
    }

    private void FindTargetIfNeeded()
    {
        if (target == null && !string.IsNullOrWhiteSpace(targetTag))
        {
            target = GameObject.FindWithTag(targetTag);
        }
    }
}
