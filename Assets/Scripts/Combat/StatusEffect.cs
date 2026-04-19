using UnityEngine;

/// <summary>
/// Base class for all status effects applied to a HealtComponent.
/// </summary>
public abstract class StatusEffect
{
    // Data set by the shell
    ///<summary>Total duration in seconds</summary>
    protected float duration;

    /// <summary>Seconds between each damage tick.</summary>
    protected float tickInterval;

    /// <summary>Damage per tick</summary>
    protected float damagePerTick;

    /// <summary>The team that applied this effect</summary>
    protected Team owner;

    /// <summary>The color the text damage will appear</summary>
    protected Color color;

    protected float elapsed = 0f;
    protected float tickTimer = 0f;
    
    /// <summary>Called once when the effect is first applied.</summary>
    public virtual void OnApply(HealthComponent target) { }

    /// <summary>
    /// Called every frame by StatusEffectHandler.
    /// Advances timers and calls Tick() at the right interval.
    /// </summary>
    public void Update(HealthComponent target, float deltaTime)
    {
        if (IsExpired()) return;

        elapsed += deltaTime;
        tickTimer += deltaTime;

        if (tickTimer >= tickInterval)
        {
            tickTimer -= tickInterval;
            Tick(target);
        }
    }

    /// <summary>Called every tickInterval - apply damage / healing here.</summary>
    protected abstract void Tick(HealthComponent target);

    /// <summary>Called once when the effect expires or is removed.</summary>
    public virtual void OnExpire(HealthComponent target) { }

    /// <summary>
    /// Called when the same effect is applied again while already active.
    /// Default : resets the duration (Refresh).
    /// Override to stack intensity instead.
    /// </summary>
    public virtual void Refresh(StatusEffect incoming) 
    {
        elapsed = 0f;
    }

    public bool IsExpired()
    {
        return elapsed >= duration;
    }

    /// <summary>The TypeEffect enum value this class handles.</summary>
    public abstract TypeEffect GetEffect();

    #region Getters
    public float GetDuration() {  return duration; } 
    public float GetTickInterval() { return tickInterval; }
    public float GetDamagePerTick() { return damagePerTick; }
    public Team GetTeam() { return owner; }
    #endregion
}
