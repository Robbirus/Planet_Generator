using UnityEngine;

/// <summary>
/// Defines all static properties of a weapon.
/// The shell (ammo) is defined separately in ShellSO.
/// Create via: Game/Weapons/Weapon
/// </summary>
[CreateAssetMenu(fileName = "New Weapon", menuName = "Game/Weapons/Weapon")]
public class WeaponSO : ScriptableObject
{
    [Header("Identity")]
    public string weaponName;
    public Sprite weaponIcon;
    public WeaponType weaponType;

    [Header("Fire Rate")]
    [Tooltip("Rounds fired per second.")]
    [Min(0.01f)]
    public float fireRate = 2f;

    [Header("Magazine")]
    public bool hasMagazine = true;
    public int magazineSize = 10;
    [Tooltip("Time in seconds to reload a full magazine.")]
    public float reloadTime = 2f;

    [Header("Guided Projectile")]
    [Tooltip("If true, fired shells will home toward the nearest enemy.")]
    public bool isGuided = false;

    [Tooltip("Turn rate of guided shells in degrees per second.")]
    public float guidedTurnRate = 120f;

    [Tooltip("Detection radius used by guided shells to find a target.")]
    public float guidedDetectionRadius = 200f;

    [Header("Default Ammo")]
    [Tooltip("The shell type loaded in this weapon by default.")]
    public ShellSO defaultShell;
}