using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIShip : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SpaceshipController spaceshipController;

    [Header("Speed")]
    [SerializeField] private TMP_Text currentSpeedText;

    [Header("Planet Info")]
    [SerializeField] private TMP_Text currentPlanetText;

    [Header("Crosshair")]
    [SerializeField] private RectTransform crosshair;
    [SerializeField] private Image reticle;

    [Header("Boost")]
    [SerializeField] private Slider boostCharge;

    // Validity checks
    private bool hasController;
    private bool hasSpeedText;
    private bool hasPlanetText;
    private bool hasCrosshair;
    private bool hasReticle;
    private bool hasBoostCharge;

    private void Awake()
    {
        // Unique checks for each reference
        hasController   = Validate(spaceshipController  != null, nameof(spaceshipController));
        hasSpeedText    = Validate(currentSpeedText     != null, nameof(currentSpeedText));
        hasPlanetText   = Validate(currentPlanetText    != null, nameof(currentPlanetText));
        hasCrosshair    = Validate(crosshair            != null, nameof(crosshair));
        hasReticle      = Validate(reticle              != null, nameof(reticle));
        hasBoostCharge  = Validate(boostCharge          != null, nameof(boostCharge));

        if (!hasController)
        {
            Debug.LogError($"[UIShip] SpaceshipController is missing", this);
            enabled = false; // Disable the script if the controller is missing
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdateSpeed();
        UpdatePlanetInfo();
        UpdateCrosshair();
        UpdateReticle();
        UpdateBoostCharge();        
    }

    private void UpdateSpeed()
    {
        if (!hasSpeedText) return;
        currentSpeedText.text = $"Speed: {spaceshipController.GetActiveForwardSpeed():0.00} m/s";
    }

    private void UpdatePlanetInfo()
    {
        if (!hasPlanetText) return;
        string planetName   = spaceshipController.GetCurrentPlanetName();
        float distance      = spaceshipController.GetPlanetDistance();
        string state        = spaceshipController.GetPlanetState();

        currentPlanetText.text = $"Current Planet: {planetName} \n" +
                                  $"Distance : {distance:0.00} m \n" +
                                  $"State : {state}";
    }

    private void UpdateCrosshair()
    {
        if (!hasCrosshair) return;

        bool locked = spaceshipController.GetPlanetState() == "Locked";
        crosshair.gameObject.SetActive(!locked);

        if (!locked)
            crosshair.position = spaceshipController.GetVirtualCursor();
    }

    private void UpdateReticle()
    {
        if (!hasReticle) return;
        string state = spaceshipController.GetPlanetState();
        switch (state)
        {
            case "None":
                reticle.gameObject.SetActive(true);
                reticle.color = Color.white;
                break;
            case "Selectable":
                reticle.color = Color.yellow;
                break;
            case "Locked":
                reticle.gameObject.SetActive(false);
                break;
        }
    }

    private void UpdateBoostCharge()
    {
        if (!hasBoostCharge) return;
        boostCharge.value = spaceshipController.GetBoostTimeRatio();
    }

    // Utilities
    private bool Validate(bool isAsigned, string fieldName)
    {
        if (isAsigned) return true;

        Debug.LogWarning($"[UIShip] '{fieldName}' is not assigned in the inspector.", this);
        return false;
        
    }
}
