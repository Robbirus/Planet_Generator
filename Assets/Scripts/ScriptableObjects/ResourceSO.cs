using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ResourceSO", menuName = "Game/Celestials/Resource Data")]
public class ResourceSO : ScriptableObject
{
    [Tooltip("Pool of resources that can appear on this planet type. The percentages are randomised at runtime.")]
    public List<ResourceType> availableResources = new();
}