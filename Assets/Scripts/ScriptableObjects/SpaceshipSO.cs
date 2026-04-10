using UnityEngine;

[CreateAssetMenu(fileName = "SpaceshipSO", menuName = "Game/SpaceShip Data")]
public class SpaceshipSO : ScriptableObject
{
    [Header("Free Movement")]
    public float forwardSpeed = 25f;
    public float strafSpeed = 7.5f;
    public float hoverSpeed = 5f;
    [Space(5)]

    [Header("Orbital Movement")]
    public float rotationSpeed = 25f;
    public Vector2 heightRange = new Vector2(50, 100);
    [Space(5)]

    [Header("Acceleration")]
    public float forwardAcceleration = 2.5f;
    public float strafAcceleration = 2f;
    public float hoverAcceleration = 2f;
    [Space(5)]

    [Header("Roll")]
    public float rollSpeed = 90f;
    public float rollAcceleration = 3.5f;
    [Space(5)]
    
    [Header("Boost")]
    public float boostMultiplier = 3f;
    public float boostAcceleration = 4f;
    public float boostDuration = 10f;
    [Space(5)]

    [Header("Boost Regeneration")]
    [Tooltip("Delay before passive regeneration start")]
    public float boostRegenDelay = 2f;
    [Tooltip("Passive regeneration speed (time per second)")]
    public float boostRegenRate = 1f;
    [Space(5)]

    [Header("Rotation")]
    public float lookRateSpeed = 90f;
    [Space(5)]

    [Header("Dead zone")]
    [Tooltip("Radius in px around the center where mouse input is ignored.")]
    public float deadZoneRadius = 50f;
}
