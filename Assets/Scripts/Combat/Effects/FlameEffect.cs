using UnityEngine;

/// <summary>
/// Burns the target for DamagePerTick every TickInterval over Duration seconds.
/// </summary>
public class FlameEffect : StatusEffect
{
    public override TypeEffect GetEffect()
    {
        return TypeEffect.FIRE;
    }

    // Default values - Override via constructor if needed
    private const float DEFAULT_DURATION        = 4f;
    private const float DEFAULT_TICK_INTERVAL   = 0.5f;
    private const float DEFAULT_DAMAGE          = 5f;

    public FlameEffect(Team owner,
                       float duration       = DEFAULT_DURATION,
                       float tickeInterval  = DEFAULT_TICK_INTERVAL,
                       float damagePerTick  = DEFAULT_DAMAGE)
    {
        this.owner          = owner;
        this.duration       = duration;
        this.tickInterval   = tickeInterval;
        this.damagePerTick  = damagePerTick;
    }

    public override void OnApply(HealthComponent target)
    {
        Debug.Log($"[FlameEffect] {target.gameObject.name} is on fire! " +
          $"({damagePerTick} dmg every {tickInterval}s for {duration}s)");

        // Hookpoint: play fire VFX / sound here
        // target.GetComponent<ParticleSystem>()?.Play();
    }

    protected override void Tick(HealthComponent target)
    {
        if (target == null || target.IsDead()) return;

        target.TakeDamage(damagePerTick);

        Debug.Log($"[FlameEffect] Burn tick on {target.gameObject.name}: -{damagePerTick} HP");
    }

    public override void OnExpire(HealthComponent target)
    {
        Debug.Log($"[FlameEffect] Fire on {target?.gameObject.name} extinguished.");

        // Hookpoint: stop fire VFX here
    }
}
