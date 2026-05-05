using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Data asset for a category of celestial objects (planets, moons, comets...)
/// </summary>
[CreateAssetMenu(fileName = "CelestialObjectDataSO", menuName = "Game/Celestials/Celestial Data")]
public class CelestialObjectDataSO : ScriptableObject
{
    [Header("Names")]
    public List<string> names = new();

    [Header("Count")]
    public Vector2 numberRange;

    [Header("Orbit")]
    public Vector2 distanceRange;
    public Vector2 orbitalSpeedRange;

    [Header("Rotation")]
    public Vector2 rotationSpeedRange;

    [Header("Physical")]
    public Vector2 massRange;
    public Vector2 densityRange;
    public float visualScale;
    [Space(10)]

    // Binary Parameters
    [Header("Binary Planet (ignored for moons / comets)")]
    [Tooltip("Probability (0-1) that a moon-less planet becomes a binary pair.")]
    [Range(0f, 1f)]
    public float binaryChance = 0.15f;

    [Tooltip("Distance between the two bodies of a binary pair.")]
    public Vector2 binarySeparationRange = new Vector2(5f, 20f);

    [Tooltip("Rotation speed of the binary pair around their barycenter (deg/s).")]
    public Vector2 binaryOrbitSpeedRange = new Vector2(15f, 60f);
    [Space(10)]

    // Comet parameters
    [Header("Comet (Only used when this SO is assigned to CometData).")]
    [Tooltip("Orbital eccentricity range. 0 = circle, close to 1 = very elongated.")]
    public Vector2 eccentricityRange = new Vector2(0.7f, 0.97f);

    [Tooltip("Orbital period in seconds.")]
    public Vector2 periodRange = new Vector2(30f, 180f);

    [Tooltip("Inclination range in degrees.")]
    public Vector2 inclinationRange = new Vector2(-60f, 60f);

    /// <summary>
    /// Returns a random name from the available list using the specified seed.
    /// </summary>
    /// <param name="seed">The seed value to initialize the random number generator.</param>
    /// <returns>A randomly selected name, or "" if no names are available.</returns>
    public string GetRandomName(System.Random rng)
    {
        if (names == null || names.Count == 0)
        {
            // Debug.LogWarning("No names available in CelestialObjectDataSO.");
            return string.Empty;
        }
        int index = rng.Next(0, names.Count);
        return names[index];
    }
}
