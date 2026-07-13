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

    [Header("Procedural Terrain (planets only)")]
    [Tooltip("Pool of ShapeSettings assets to pick randomly when spawning a terrain planet.")]
    public List<ShapeSettings> shapeSettingsOptions = new();
    [Tooltip("Pool of ColourSettings assets to pick randomly.")]
    public List<ColourSettings> colourSettingsOptions = new();

    [Header("Terrain Visual Size")]
    public Vector2 terrainRadiusRange = new Vector2(80f, 400f);

    // Binary Parameters
    [Header("Binary Planet")]
    [Range(0f, 1f)]
    public float binaryChance = 0.15f;
    public Vector2 binarySeparationRange = new Vector2(5f, 20f);
    public Vector2 binaryOrbitSpeedRange = new Vector2(15f, 60f);

    // Comet parameters
    [Header("Comet")]
    public Vector2 eccentricityRange = new Vector2(0.7f, 0.97f);
    public Vector2 periodRange = new Vector2(30f, 180f);
    public Vector2 inclinationRange = new Vector2(-60f, 60f);

    public string GetRandomName(System.Random rng)
    {
        if (names == null || names.Count == 0) return string.Empty;
        return names[(int)SeedManager.Range(0, names.Count, rng)];
    }

    public ShapeSettings GetRandomShapeSettings(System.Random rng)
    {
        if (shapeSettingsOptions == null || shapeSettingsOptions.Count == 0) return null;
        return shapeSettingsOptions[(int)SeedManager.Range(0, shapeSettingsOptions.Count, rng)];
    }

    public ColourSettings GetRandomColourSettings(System.Random rng)
    {
        if (colourSettingsOptions == null || colourSettingsOptions.Count == 0) return null;
        return colourSettingsOptions[(int)SeedManager.Range(0, colourSettingsOptions.Count, rng)];
    }

    /// <summary>True if terrain pools are configured.</summary>
    public bool HasTerrainOptions =>
        shapeSettingsOptions != null && shapeSettingsOptions.Count > 0 &&
        colourSettingsOptions != null && colourSettingsOptions.Count > 0;

    /// <summary>
    /// Returns a random terrain visual radius in world units.
    /// Uses terrainRadiusRange if configured (non-zero), otherwise falls back
    /// to the mass/density formula so non-terrain bodies are unaffected.
    /// </summary>
    public float GetRandomTerrainRadius(System.Random rng)
    {
        if (terrainRadiusRange.x <= 0f && terrainRadiusRange.y <= 0f)
            return -1f; // signal: use the old formula

        return SeedManager.Range(terrainRadiusRange, rng);
    }
}