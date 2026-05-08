using UnityEngine;

public class AcidEffect : StatusEffect
{
    public override TypeEffect GetEffect()
    {
        return TypeEffect.ACID;
    }

    private float slowMultiplier;

    public AcidEffect(Team owner, StatusEffectSO data)
    {
        this.owner = owner;
        this.duration = data.duration;
        this.tickInterval = data.tickInterval;
        this.damagePerTick = data.damagePerTick;
        this.slowMultiplier = data.slowMultiplier;
        this.color = data.color;
    }

    public override void OnApply(HealthComponent target)
    {
        Debug.Log($"[Acid] {target.gameObject.name} corroded!");
        target.GetComponentInParent<EnemyEffectable>()?.Slow(slowMultiplier, duration);
    }

    protected override void Tick(HealthComponent target)
    {
        if (CannotDamage(target)) return;

        target.TakeDamage(damagePerTick, this.color, true, false);

        Debug.Log($"[FlameEffect] Burn tick on {target.gameObject.name}: -{damagePerTick} HP");
    }

    protected override bool CannotDamage(HealthComponent target)
    {
        return target == null || target.IsDead() || target.GetArmorType() == ArmorType.INDESTRUCTIBLE;
    }
}
