using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

/// <summary>
/// Displays the top 3 resources of the nearest / locked planet on the HUD
/// </summary>
public class PlanetResourceHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlanetLockSystem planetLockSystem;
    [Space(5)]

    [Header("Resources HUD")]
    [SerializeField] private GameObject resourceHUD;
    [Space(5)]

    [Header("Planet name label")]
    [SerializeField] private TMP_Text planetNameText;
    [Space(5)]

    [Header("Resource rows (top 3)")]
    [SerializeField] private TMP_Text resourceText1;
    [SerializeField] private TMP_Text resourceText2;
    [SerializeField] private TMP_Text resourceText3;
    [Space(5)]

    // Validity Check
    private bool hasLockSystem;
    private bool hasPanel;
    private bool hasNameText;

    // Cache to avoid to rebuild every frame when the planet hasn't changed
    private Transform lastPlanet;

    private void Awake()
    {
        hasLockSystem = Validate(planetLockSystem != null, nameof(planetLockSystem));
        hasPanel = Validate(resourceHUD != null, nameof(resourceHUD));
        hasNameText = Validate(planetNameText  != null, nameof(planetNameText));
    }

    private void Update()
    {
        if (!hasLockSystem) return;

        Transform activePlanet = planetLockSystem.GetActivePlanet();

        // Hide panel when no planet is in range
        if (activePlanet == null)
        {
            SetPanelVisible(false);
            lastPlanet = null;
            return;
        }

        SetPanelVisible(true);

        // Only rebuild text when the planet actually changes
        if (activePlanet == lastPlanet) return;
        lastPlanet = activePlanet;

        RefreshDisplay(activePlanet);
    }

    private void RefreshDisplay(Transform activePlanet)
    {
        // Planet Name
        if (hasNameText)
        {
            planetNameText.text = activePlanet.name;
        }

        // Get the CelestialBody to read Resources
        CelestialBody body = activePlanet.GetComponent<CelestialBody>();

        if(body == null)
        {
            // Planet has no resources data - clears rows
            SetRow(resourceText1, "-", 0f);
            SetRow(resourceText2, "-", 0f);
            SetRow(resourceText3, "-", 0f);
            return;
        }

        // Sort all resources by percentage descending, keep top 3
        List<ResourceDistribution> top3 = body.GetResourceDistributions()
            .OrderByDescending(r => r.percentage)
            .Take(3)
            .ToList();

        SetRow(resourceText1, top3.Count > 0 ? top3[0].resourceType.ToString() : "-",
                              top3.Count > 0 ? top3[0].percentage : 0f);

        SetRow(resourceText2, top3.Count > 1 ? top3[1].resourceType.ToString() : "-",
                              top3.Count > 1 ? top3[1].percentage : 0f);

        SetRow(resourceText3, top3.Count > 2 ? top3[2].resourceType.ToString() : "-",
                              top3.Count > 2 ? top3[2].percentage : 0f);

    }


    private void SetRow(TMP_Text label, string resourceName, float percentage)
    {
        if(label == null) return;
        
        if(percentage <= 0f)
        {
            label.text = "-";
            return;
        }

        label.text = $"{resourceName} {percentage:0.#}%";
    }

    private void SetPanelVisible(bool visible)
    {
        if(hasPanel && resourceHUD.activeSelf != visible)
        {
            resourceHUD.SetActive(visible);
        }
    }

    // Utility

    private bool Validate(bool assigned, string fieldName)
    {
        if (assigned) return true;
        Debug.LogWarning($"[PlanetResourceHUD] '{fieldName}' is not assigned in the Inspector.", this);
        return false;
    }
}
