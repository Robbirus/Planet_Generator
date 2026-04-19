/// <summary>
/// Creates the correct StatusEffect subclass for a given TypeEffect.
/// This is the only place that maps enum -> class.
/// to add a new effect: create the class, then add a case here.
/// </summary>
public static class StatusEffectFactory
{
    /// <summary>
    /// Returns a new StatusEffect instance for the given type, or null if NONE.
    /// </summary>
    public static StatusEffect Create(TypeEffect type, Team owner, StatusEffectSO data)
    {
        if (data == null || type == TypeEffect.NONE) return null;

        return type switch
        {
            TypeEffect.FIRE         => new FlameEffect(owner, data),
            TypeEffect.ACID         => null, // TODO: implement AcidEffect
            TypeEffect.ARC          => null, // TODO: implement ArcEffect
            TypeEffect.EXPLOSION    => null, // TODO: implement ExplosionEffect
            TypeEffect.IMPACT       => null, // TODO: implement ImpactEffect
            TypeEffect.LASER        => null, // TODO: implement LaserEffect
            TypeEffect.CHEMICAL     => null, // TODO: implement ChemicalEffect
            TypeEffect.NONE         => null,
            _                       => null
        };
    }
}
