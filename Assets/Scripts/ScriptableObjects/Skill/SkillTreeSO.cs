using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Container for the full skill tree.
/// Holds all nodes and exposes helpers for validation.
/// </summary>
[CreateAssetMenu(fileName = "newSkillTree", menuName = "Game/Skill Tree/Skill Tree")]
public class SkillTreeSO : ScriptableObject
{
    [Tooltip("All nodes that belong to this tree. Order does not matter.")]
    public List<SkillNodeSO> nodes = new();

    ///<summary>
    /// Returns a list of duplicate IDs - usefull for the custom Editor validator
    /// </summary>
    public List<string> FindDuplicateIDs()
    {
        HashSet<string> seen = new();
        List<string> duplicates = new();

        foreach (SkillNodeSO node in nodes)
        {
            if(node == null) continue;
            if (!seen.Add(node.id))
            {
                duplicates.Add(node.id);
            }
        }

        return duplicates;
    }

    ///<summary>
    /// Returns all nodes that have no prerequisites (the roots of the tree).
    /// </summary>
    public List<SkillNodeSO> GetRootNodes()
    {
        List<SkillNodeSO> roots = new();
        foreach (SkillNodeSO node in nodes)
        {
            if(node != null && node.prerequisites.Count == 0)
            {
                roots.Add(node);
            }
        }

        return roots;
    }

    /// <summary>
    /// Returns all nodes that list the given node as a direct prerequisite.
    /// </summary>
    public List<SkillNodeSO> GetChildren(SkillNodeSO parent)
    {
        List<SkillNodeSO> children = new();
        foreach(SkillNodeSO node in nodes)
        {
            if(node == null) continue;
            if (node.prerequisites.Contains(parent))
            {
                children.Add(node);
            }
        }

        return children;
    }
}
