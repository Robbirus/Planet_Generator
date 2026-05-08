using UnityEngine;

public class EnemyMovement : MonoBehaviour
{
    [Header("Path")]
    [SerializeField] private Transform pointA;
    [SerializeField] private Transform pointB;
    [Tooltip("Used when pointA / pointB are not assigned.")]
    [SerializeField] private float pingPongDistance = 50000f;
    [Space(10)]

    [Header("Speed")]
    [SerializeField] private float baseSpeed = 500f;
    [Space(10)]

    [Header("Debug - current state")]
    [SerializeField] private float currentSpeed;
    [SerializeField] private bool isStunned;
    [SerializeField] private float slowMultiplier = 1f;

    private Vector3 targetA;
    private Vector3 targetB;
    private Vector3 currentTarget;

    private float stunTimer = 0f;
    private float slowTimer = 0f;
    private float pendingSlowMultiplier = 1f;

    private void Start()
    {
        targetA = pointA != null ? pointA.position
                                 : transform.position + transform.right * pingPongDistance;

        targetB = pointB != null ? pointB.position
                                 : transform.position - transform.right * pingPongDistance;

        currentTarget = targetB;
    }

    private void Update()
    {
        TickTimers();

        if (isStunned) return;

        currentSpeed = baseSpeed * slowMultiplier;

        transform.position = Vector3.MoveTowards(
            transform.position, currentTarget, currentSpeed * Time.deltaTime);

        // Swap target when reached
        if(Vector3.Distance(transform.position, currentTarget) < 0.1f)
        {
            currentTarget = currentTarget == targetA ? targetB : targetA;
        }
    }

    private void TickTimers()
    {
        if(stunTimer > 0f)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0f) isStunned = false;
        }

        if(slowTimer > 0f)
        {
            slowTimer -= Time.deltaTime;
            if(slowTimer <= 0f) slowMultiplier = 1f;
        }
    }

    public void ApplySlow(float multiplier, float duration)
    {
        pendingSlowMultiplier = Mathf.Min(pendingSlowMultiplier, multiplier);
        slowMultiplier = pendingSlowMultiplier;
        slowTimer = Mathf.Max(slowTimer, duration);

        Debug.Log($"[EnemyMovement] Slowed ~ {multiplier:0.00} for {duration:0.0}s", this);
    }

    public void ApplyStun(float duration)
    {
        isStunned = true;
        stunTimer = Mathf.Max(stunTimer, duration);

        Debug.Log($"[EnemyMovement] Stunned for {duration:0.0}s", this);
    }

    public void ApplyKnockBack(Vector3 direction, float force)
    {
        transform.position += direction.normalized * force * Time.deltaTime * 10f;

        Debug.Log($"[EnemyMovement] Knockback dir={direction} force={force}");
    }

    public float GetBaseSpeed()
    {
        return baseSpeed;
    }

    public float GetCurrentSpeed()
    {
        return currentSpeed;
    }

    public bool IsStunned()
    {
        return isStunned;
    }

    public bool IsSlowed()
    {
        return slowMultiplier < 1f;
    }
}
