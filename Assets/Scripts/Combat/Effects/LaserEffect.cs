using UnityEngine;

public class LaserEffect : StatusEffect
{
    public override TypeEffect GetEffect()
    {
        return TypeEffect.LASER;
    }

    public LaserEffect(Team owner, StatusEffectSO data)
    {
        this.owner = owner;
        this.duration = data.duration;
        this.tickInterval = data.tickInterval;
        this.damagePerTick = data.damagePerTick;
        this.color = data.color;
    }

    public override void OnApply(HealthComponent target)
    {
        Debug.Log($"[Laser] {target.gameObject.name} is burning from laser!"); 
    }

    protected override bool CannotDamage(HealthComponent target)
    {
        return target == null || target.IsDead() || target.GetArmorType() == ArmorType.INDESTRUCTIBLE;
    }

    protected override void Tick(HealthComponent target)
    {
        if (CannotDamage(target)) return;

        target.TakeDamage(damagePerTick, color, true, false);
    }
}
