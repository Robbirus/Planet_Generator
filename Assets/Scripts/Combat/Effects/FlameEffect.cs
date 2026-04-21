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

    private StatusEffectSO data;

    public FlameEffect(Team owner, StatusEffectSO data)
    {
        this.owner          = owner;
        this.data           = data;
        this.color          = data.color;
        this.duration       = data.duration;
        this.tickInterval   = data.tickInterval;
        this.damagePerTick  = data.damagePerTick;
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
        if (target == null || target.IsDead() || target.GetArmorType() == ArmorType.INDESTRUCTIBLE) return;

        target.TakeDamage(damagePerTick, color, true);

        Debug.Log($"[FlameEffect] Burn tick on {target.gameObject.name}: -{damagePerTick} HP");
    }

    public override void OnExpire(HealthComponent target)
    {
        Debug.Log($"[FlameEffect] Fire on {target?.gameObject.name} extinguished.");

        // Hookpoint: stop fire VFX here
    }
}
