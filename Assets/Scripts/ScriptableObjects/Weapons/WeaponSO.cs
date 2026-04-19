using UnityEngine;
using System.Collections.Generic;

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
    public List<AudioClip> fireSounds;
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

    [Header("Critical Hits")]
    [Range(0, 100)]
    [Tooltip("Base crit chance (0-100).")]
    public int critChance = 2;
    [Min(1f)]
    [Tooltip("Crit damage multiplier (1.1 = +10%).")]
    public float critCoef = 1.1f;

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