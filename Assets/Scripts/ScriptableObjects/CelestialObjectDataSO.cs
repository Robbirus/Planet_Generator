using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CelestialObjectDataSO", menuName = "Game/Celestials/Celestial Data")]
public class CelestialObjectDataSO : ScriptableObject
{
    public List<string> names;
    public Vector2 numberRange;
    public Vector2 distanceRange;
    public Vector2 orbitalSpeedRange;
    public Vector2 rotationSpeedRange;
    public Vector2 massRange;
    public Vector2 densityRange;


    /// <summary>
    /// Returns a random name from the available list using the specified seed.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random number generator.</param>
    /// <returns>A randomly selected name, or "" if no names are available.</returns>
    public string GetRandomName()
    {
        if (names == null || names.Count == 0)
        {
            // Debug.LogWarning("No names available in CelestialObjectDataSO.");
            return "";
        }
        int index = UnityEngine.Random.Range(0, names.Count);
        return names[index];
    }
}
