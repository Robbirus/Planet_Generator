using UnityEngine;

public class EnemyEffectable : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private EnemyMovement movement;

    private void Awake()
    {
        if(movement == null)
        {
            movement = GetComponent<EnemyMovement>();
        }
    }

    public void Slow(float multiplier, float duration)
    {
        movement?.ApplySlow(multiplier, duration);
    }

    public void Stun(float duration)
    {
        movement?.ApplyStun(duration);
    }

    public void KnockBack(Vector3 direction, float force)
    {
        movement?.ApplyKnockBack(direction, force);
    }

    public void ReduceArmor(HealthComponent target, int tiers, float duration)
    {
        if(target == null)
        {
            target = GetComponentInChildren<HealthComponent>();
        }

        target?.ApplyArmorReduction(tiers, duration);
    }
}
