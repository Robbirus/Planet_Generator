using UnityEngine;

public class ArcEffect : StatusEffect
{
    public override TypeEffect GetEffect()
    {
        return TypeEffect.ARC;
    }

    private float stunDuration;

    public ArcEffect(Team owner, StatusEffectSO data)
    {
        this.owner = owner;
        this.duration = data.duration;
        this.tickInterval = data.tickInterval;
        this.stunDuration = data.stunDuration;
        this.color = data.color;
    }

    public override void OnApply(HealthComponent target)
    {
        Debug.Log($"[Arc] {target.gameObject.name} stunned for {stunDuration}s!");
        target.GetComponentInParent<EnemyEffectable>()?.Stun(stunDuration);
    }

    protected override bool CannotDamage(HealthComponent target)
    {
        return target == null || target.IsDead() || target.GetArmorType() == ArmorType.INDESTRUCTIBLE;
    }

    // No Damage
    protected override void Tick(HealthComponent target){ }

    public override void OnExpire(HealthComponent target)
    {
        Debug.Log($"[Arc] Stun expired on {target?.gameObject.name}");
    }
}
