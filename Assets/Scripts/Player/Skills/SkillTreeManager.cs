using System;
using System.Collections.Generic;
using UnityEngine;

public class SkillTreeManager : MonoBehaviour
{
    [Header("Data")]
    [Tooltip("The skill tree asset.")]
    [SerializeField] private SkillTreeSO tree;
    [Space(10)]

    [Tooltip("Original ship stats — used as the base for all calculations. Never modified.")]
    [SerializeField] private SpaceshipSO baseData;
    [Space(10)]

    [Header("References")]
    [SerializeField] private ShipInventory inventory;
    [SerializeField] private SpaceshipController spaceshipController;
    [SerializeField] private ResourceHarvester harvester;
    [Space(10)]

    [Header("Debug — Unlocked Nodes")]
    [SerializeField] private List<string> unlockedIds = new();
    [Space(10)]

    // Fast lookup set (mirrors unlockedIds list for O(1) checks)
    private HashSet<string> unlockedSet = new();

    /// <summary>Fires after any node is successfully unlocked.</summary>
    public event Action<SkillNodeSO> OnNodeUnlocked;

    private void Awake()
    {
        // Rebuild the HashSet from the serialized list (survives domain reloads in Editor)
        foreach(string id in unlockedIds)
        {
            unlockedSet.Add(id);
        }

        ApplyAllEffects();
    }

    /// <summary>
    /// Tries to unlock a node. Returns true on success.
    /// Fails if prerequisites are not met or the player can't afford the cost.
    /// </summary>
    public bool TryUnlock(SkillNodeSO node)
    {
        if (node == null) return false;
        if (unlockedSet.Contains(node.id)) return false;
        if (!node.ArePrerequisitesMet(unlockedSet)) return false;
        if (!node.CanAfford(inventory)) return false;

        node.DeductCosts(inventory);

        unlockedSet.Add(node.id);
        unlockedIds.Add(node.id);       // keep the serialized list in sync

        ApplyAllEffects();              // recalculate from scratch to stay consistent
        OnNodeUnlocked?.Invoke(node);

        Debug.Log($"[SkillTreeManager] Unlocked: {node.displayName}");
        return true;
    }

    public bool IsUnlocked(SkillNodeSO node)
    {
        return node != null && unlockedSet.Contains(node.id);
    }

    public bool IsAvailable(SkillNodeSO node)
    {
        return node != null && !unlockedSet.Contains(node.id) && node.ArePrerequisitesMet(unlockedSet);
    }

    /// <summary>
    /// Recalculates all stats from the base SpaceshipSO + every unlocked effect.
    /// Called once at startup and after each unlock.
    /// </summary>
    private void ApplyAllEffects()
    {
        if (baseData == null || spaceshipController == null) return;

        // Start from base values
        float forwardSpeed = baseData.forwardSpeed;
        float strafeSpeed = baseData.strafSpeed;
        float hoverSpeed = baseData.hoverSpeed;
        float rotationSpeed = baseData.rotationSpeed;
        float rollSpeed = baseData.rollSpeed;
        float lookRateSpeed = baseData.lookRateSpeed;
        float boostMultiplier = baseData.boostMultiplier;
        float boostDuration = baseData.boostDuration;
        float boostRegenRate = baseData.boostRegenRate;
        float boostRegenDelay = baseData.boostRegenDelay;
        float harvestRate = harvester != null ? harvester.GetBaseHarvestRate() : 10f;
        float inventoryCapacity = inventory != null ? inventory.GetMaxCapacityPerResource() : 200f;

        // Accumulate effects from every unlocked node
        foreach (SkillNodeSO node in tree.nodes)
        {
            if (node == null || !unlockedSet.Contains(node.id)) continue;

            foreach (SkillEffect effect in node.effects)
            {
                switch (effect.type)
                {
                    case SkillEffectType.ForwardSpeed:      forwardSpeed        = Apply(forwardSpeed, effect);          break;
                    case SkillEffectType.StrafeSpeed:       strafeSpeed         = Apply(strafeSpeed, effect);           break;
                    case SkillEffectType.HoverSpeed:        hoverSpeed          = Apply(hoverSpeed, effect);            break;
                    case SkillEffectType.RotationSpeed:     rotationSpeed       = Apply(rotationSpeed, effect);         break;
                    case SkillEffectType.RollSpeed:         rollSpeed           = Apply(rollSpeed, effect);             break;
                    case SkillEffectType.LookRateSpeed:     lookRateSpeed       = Apply(lookRateSpeed, effect);         break;
                    case SkillEffectType.BoostMultiplier:   boostMultiplier     = Apply(boostMultiplier, effect);       break;
                    case SkillEffectType.BoostDuration:     boostDuration       = Apply(boostDuration, effect);         break;
                    case SkillEffectType.BoostRegenRate:    boostRegenRate      = Apply(boostRegenRate, effect);        break;
                    case SkillEffectType.BoostRegenDelay:   boostRegenDelay     = Apply(boostRegenDelay, effect);       break;
                    case SkillEffectType.HarvestRate:       harvestRate         = Apply(harvestRate, effect);           break;
                    case SkillEffectType.InventoryCapacity: inventoryCapacity   = Apply(inventoryCapacity, effect);     break;
                }
            }
        }

        // Push computed values to the runtime systems
        spaceshipController.SetStats(
            forwardSpeed, strafeSpeed, hoverSpeed,
            rotationSpeed, rollSpeed, lookRateSpeed,
            boostMultiplier, boostDuration, boostRegenRate, boostRegenDelay
        );

        if (harvester != null) harvester.SetBaseHarvestRate(harvestRate);
        if (inventory != null) inventory.SetMaxCapacityPerResource(inventoryCapacity);
    }

    /// <summary>Applies a single SkillEffect to a base value.</summary>
    private float Apply(float baseValue, SkillEffect effect)
    {
        return effect.mode == SkillModifierMode.Flat
            ? baseValue + effect.value
            : baseValue * (1f + effect.value);
    }
}
