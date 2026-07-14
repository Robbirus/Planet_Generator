using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ShapeSettings", menuName = "Game/PlanetTerrain/ShapeSettings")]
public class ShapeSettings : ScriptableObject
{
    public float planetRadius = 3;
    public NoiseLayer[] noiseLayers;

    [System.Serializable]
    public class NoiseLayer // controls an individual layer of noise
    {
        public bool enabled = true;
        public bool useFirstLayerAsMask; // determines if we can use the first layer (ie continents) as a mask for subsequent layers
        public NoiseSettings noiseSettings;
    }
}
