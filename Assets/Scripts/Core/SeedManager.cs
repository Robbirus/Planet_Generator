using System.Collections.Generic;

/// <summary>
/// Manages the master seed for the current game session.
///
/// USAGE IN MAIN MENU (before loading the game scene):
///   SeedManager.Randomize();
///   SeedManager.SetSeedFromString(inputField.text);
///   SeedManager.SetSeed(42);
///
/// USAGE IN SolarSystemGenerator:
///   System.Random stellarRNG   = SeedManager.GetRNG("stellar");
///   System.Random planetaryRNG = SeedManager.GetRNG("planetary");
///   System.Random lunarRNG     = SeedManager.GetRNG("lunar");
/// </summary>
public static class SeedManager
{
    private static int currentSeed = 0;
    private static Dictionary<string, System.Random> rngCache = new();

    /// <summary>Sets the master seed explicitly.</summary>
    public static void SetSeed(int seed)
    {
        currentSeed = seed;
        UnityEngine.Debug.Log($"[SeedManager] Seed set to {currentSeed}");
    }

    /// <summary>Returns the current master seed.</summary>
    public static int GetSeed() { return currentSeed; }

    /// <summary>
    /// Parses a seed from a string (for text input fields).
    /// If the string is a valid integer, it is used directly.
    /// Otherwise its hash code is used.
    /// </summary>
    public static void SetSeedFromString(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
        {
            Randomize();
            return;
        }

        if (int.TryParse(input, out int parsed))
            SetSeed(parsed);
        else
            SetSeed(input.GetHashCode());
    }

    /// <summary>Generates and stores a new random seed.</summary>
    public static void Randomize(bool isDebugMode = false)
    {
        currentSeed = UnityEngine.Random.Range(int.MinValue, int.MaxValue);
        if (isDebugMode)
        {
            UnityEngine.Debug.Log($"[SeedManager] Random seed generated: {currentSeed}");
        }
    }

    /// <summary>
    /// Returns a System.Random derived from the master seed
    /// and an optional salt string.
    ///
    /// Each unique salt produces a different but reproducible sequence.
    /// </summary>
    public static System.Random GetRNG(string salt = "")
    {
        if (!rngCache.ContainsKey(salt))
        {
            int derivedSeed = string.IsNullOrEmpty(salt)
                ? currentSeed
                : currentSeed ^ salt.GetHashCode();

            rngCache[salt] = new System.Random(derivedSeed);
        }

        return rngCache[salt];
    }

    /// <summary>
    /// Returns a number between min and max value with the given rng.
    /// </summary>
    /// <param name="min">Minimum value</param>
    /// <param name="max">Maximum value</param>
    /// <param name="rng">System.Random generator</param>
    /// <returns></returns>
    public static float Range(float min, float max, System.Random rng)
    {
        return (float)(rng.NextDouble() * (max - min) + min);
    }
}