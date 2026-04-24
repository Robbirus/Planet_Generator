using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NoiseSettings
{
    public enum FilterType {  Simple, Rigid };
    public FilterType filterType;

    public float strength = 1;
    [Range(1,8)]
    public int numLayers = 1; // number of layers of noise
    public float baseRoughness = 1;
    public float roughness = 2;
    public float persistence = .5f; // controls how fast the amplitude of the noise decreases with each layer
    public Vector3 centre;
    public float minValue; // gets the terrain to recede into the base sphere of the planet
}
