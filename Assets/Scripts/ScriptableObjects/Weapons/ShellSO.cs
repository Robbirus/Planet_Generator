using UnityEngine;

[CreateAssetMenu(fileName = "New Shell", menuName = "Game/Weapons/Shell")]
public class ShellSO : ScriptableObject
{
    [Header("Visuals")]
    public Color color;
    public Sprite shellImage;

    [Header("Type")]
    public WeaponType weaponType;

    [Header("Ballistics")]
    public int velocity;
    public float lifeTime;

    [Header("Damage")]
    public float standardDamage;
    public float durableDamage;

    [Header("Armor Penetration")]
    [Tooltip("The highest armor class thie shell can fully penetrate")]
    public ArmorType armorPen;

    [Header("Status Effects")]
    [Tooltip("Special effect applied on hit.")]
    public StatusEffectSO effectData;

    /// <summary>Convenience - returns NONE if no effectData assigned</summary>
    public TypeEffect GetTypeEffect()
    {
        return effectData != null ? effectData.effectType : TypeEffect.NONE;
    }
}