using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceSO", menuName = "Game/Celestials/Resource Data")]
public class ResourceSO : ScriptableObject
{
    [Tooltip("Pool of resources that can appear on this planet. " +
             "Percentages are randomised at runtime; rules shift the weights.")]
    public List<ResourceType> availableResources = new();

    [Header("Resource Rules")]
    [Tooltip("Conditions that boost certain resources based on planetary characteristics. " +
             "Multiple rules can be active simultaneously — their multipliers stack additively.")]
    public List<PlanetResourceRuleSO> rules = new();
}