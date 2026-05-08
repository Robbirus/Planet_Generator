using UnityEngine;

public class ImpactEffect : StatusEffect
{
    public override TypeEffect GetEffect()
    {
        return TypeEffect.IMPACT;
    }

    private float knockbackForce;
    private float stunDuration;

    public ImpactEffect(Team owner, StatusEffectSO data)
    {
        this.owner = owner;
        this.duration = 0.1f; // Expires almost immediately
        this.tickInterval = 999f; // No tick
        this.stunDuration = data.impactStunDuration;
        this.knockbackForce = data.knockBackForce;
        this.color = data.color;
    }

    public override void OnApply(HealthComponent target){ }

    protected override bool CannotDamage(HealthComponent target)
    {
        return target == null || target.IsDead() || target.GetArmorType() == ArmorType.INDESTRUCTIBLE;
    }

    protected override void Tick(HealthComponent target){ }
}
