using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all active StatusEffects on a GameObject.
/// Auto-added by HealthComponent.ApplyEffect() - no manual setup needed.
///
/// One handler per GameObject. Effects are keyed by TypeEffect so the same
/// effect type can only be active once at a time (refresh instead of stack).
/// </summary>
public class StatusEffectHandler : MonoBehaviour
{
    // Key = TypeEffect, Value = active Effect
    private Dictionary<TypeEffect, StatusEffect> activesEffects = new();

    // Cached reference set by HealthComponent
    private HealthComponent owner;

    public void Init(HealthComponent target)
    {
        owner = target;
    }

    private void Update()
    {
        if (activesEffects.Count == 0) return;

        // Collect expired Keys - can't modify dict durint iteration
        List<TypeEffect> toRemove = null;

        foreach (var pair in activesEffects)
        {
            pair.Value.Update(owner, Time.deltaTime);

            if (pair.Value.IsExpired())
            {
                pair.Value.OnExpire(owner);
                toRemove ??= new List<TypeEffect>();
                toRemove.Add(pair.Key);
            }
        }

        if (toRemove == null) return;
        foreach(TypeEffect key in toRemove)
        {
            activesEffects.Remove(key);
        }
    }

    /// <summary>
    /// Applies an effect from a shell.
    /// If the same effect type is already active, refreshes it instead of stacking.
    /// </summary>
    public void Apply(TypeEffect type, Team team)
    {
        StatusEffect incoming = StatusEffectFactory.Create(type, team);
        if (incoming == null) return;

        if(activesEffects.TryGetValue(type, out StatusEffect existing))
        {
            // Already burning/poisoned - just refresh the timer
            existing.Refresh(incoming);
        }
        else
        {
            activesEffects[type] = incoming;
            incoming.OnApply(owner);
        }
    }

    /// <summary>Removes a specific effect immediately.</summary>
    public void Remove(TypeEffect type)
    {
        if (!activesEffects.TryGetValue(type, out StatusEffect effect)) return;
        effect.OnExpire(owner);
        activesEffects.Remove(type);
    }

    /// <summary>Removes all active effect immediately.</summary>
    public void RemoveAll()
    {
        foreach(var pair in activesEffects)
        {
            pair.Value.OnExpire(owner);
        }

        activesEffects.Clear();
    }

    public bool HasEffect(TypeEffect effect)
    {
        return activesEffects.ContainsKey(effect);
    }
}
