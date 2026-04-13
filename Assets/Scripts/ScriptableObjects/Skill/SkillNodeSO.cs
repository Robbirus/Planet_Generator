using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data for a single node in the tree
/// Create via Game/Skill Tree/Skill Node
/// </summary>
[CreateAssetMenu(fileName = "newNode", menuName = "Game/Skill Tree/Skill Node")]
public class SkillNodeSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Unique ID used to track which skills are unlocked. Never change this at runtime.")]
    public string id;
    public string displayName;
    [TextArea(2, 5)]
    public string description;
    public Sprite icon;
    [Space(10)]

    [Header("Cost")]
    [Tooltip("Resources required to unlock this node.")]
    public List<SkillCost> costs = new();
    [Space(10)]

    [Header("Effects")]
    [Tooltip("Stat modifications applied when this node is unlocked.")]
    public List<SkillEffect> effects = new();
    [Space(10)]

    [Header("Prerequisites")]
    [Tooltip("All these nodes must be unlocked before this one becomes available.")]
    public List<SkillNodeSO> prerequisites = new();
    [Space(10)]

    [Header("Canvas Position")]
    [Tooltip("Position of this node inside the Skill Tree Canvas (local space).")]
    public Vector2 canvasPosition;

    ///<summary>Returns true if all prerequisites are in the provide unlocked set.</summary>
    public bool ArePrerequisitesMet(HashSet<string> unlockedIDs)
    {
        foreach(SkillNodeSO prereq in prerequisites)
        {
            if (prereq == null) continue;
            if (!unlockedIDs.Contains(prereq.id)) return false;
        }
        return true;
    }

    ///<summary>Returns true if the inventory can afford all costs.</summary>
    public bool CanAfford(ShipInventory inventory)
    {
        foreach(SkillCost cost in costs)
        {
            if(inventory.Get(cost.resourceType) < cost.amount) return false;
        }

        return true;
    }

    ///<summary>Deducts all costs from the inventory. Call only after CanAfford returns true.</summary>
    public void DeductCosts(ShipInventory inventory)
    {
        foreach(SkillCost cost in costs)
        {
            inventory.Spend(cost.resourceType, cost.amount);
        }
    }

    private void OnValidate()
    {
        // Auto-Generate an ID from the asset name if empty
        if (string.IsNullOrEmpty(id))
        {
            id = name;
        }
    }
}

///<summary>One resource cost entry for a skill node.</summary>
[Serializable]
public struct SkillCost
{
    public ResourceType resourceType;
    [Min(0f)]
    public float amount;
}
