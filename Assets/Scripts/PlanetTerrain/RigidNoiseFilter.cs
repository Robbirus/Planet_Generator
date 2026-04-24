using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Noise to create ridge-like terrain
public class RigidNoiseFilter : INoiseFilter
{
    NoiseSettings.RigidNoiseSettings settings;
    Noise noise = new Noise();

    public RigidNoiseFilter(NoiseSettings.RigidNoiseSettings settings)
    {
        this.settings = settings;
    }
    public float Evaluate(Vector3 point)
    {
        float noiseValue = 0;
        float frequency = settings.baseRoughness;
        float amplitude = 1;
        float weight = 1;

        for (int i = 0; i < settings.numLayers; i++)
        {
            float value = 1 - Mathf.Abs(noise.Evaluate(point * frequency + settings.centre));
            value *= value;
            value *= weight;
            weight = Mathf.Clamp01(value * settings.weightMultiplier); // allows the noise to become more detailed as the elevation increases

            noiseValue += value * amplitude;
            frequency *= settings.roughness;
            amplitude *= settings.persistence;
        }

        noiseValue = Mathf.Max(0, noiseValue - settings.minValue);
        return noiseValue * settings.strength;
    }
}
