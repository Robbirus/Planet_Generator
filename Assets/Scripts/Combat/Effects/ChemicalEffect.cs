using UnityEngine;

public class ChemicalEffect : StatusEffect
{
    public override TypeEffect GetEffect()
    {
        return TypeEffect.CHEMICAL;
    }

    private int armorReductionTiers;
    private HealthComponent affectedPart;

    public ChemicalEffect(Team owner, StatusEffectSO data)
    {
        this.owner = owner;
        this.duration = data.duration;
        this.tickInterval = data.tickInterval;
        this.damagePerTick = data.damagePerTick;
        this.armorReductionTiers = data.armorReductionTiers;
    }

    public override void OnApply(HealthComponent target)
    {
        Debug.Log($"[Chemical] {target.gameObject.name} armor corroded by {armorReductionTiers} tier(s)!");

        affectedPart = target;
        affectedPart.GetComponentInParent<EnemyEffectable>()?.ReduceArmor(target, armorReductionTiers, duration);
    }

    protected override bool CannotDamage(HealthComponent target)
    {
        return target == null || target.IsDead() || target.GetArmorType() == ArmorType.INDESTRUCTIBLE;
    }

    protected override void Tick(HealthComponent target)
    {
        if (CannotDamage(target)) return;

        target.TakeDamage(damagePerTick, color, true);
    }

    public override void OnExpire(HealthComponent target)
    {
        Debug.Log($"[Chemical] Chemical expired on {target?.gameObject.name}");
    }
}
