using System;
using UnityEngine;

/// <summary>
/// Health and armor for a single destructible part (or for the player ship).
/// Implements IDamageable - attach on each hittable collider
/// 
/// ENEMY SETUP:
///     Each enemy part gets one HealthComponent
///     EnemyHealth on the root GO references all parts and the Main Frame
///     
/// PLAYER SETUP:
///     One HealthComponent on the ship root, Team = Player
/// </summary>
public class HealthComponent : MonoBehaviour, IDamageable
{
    [Header("Team")]
    [SerializeField] private Team team = Team.Enemy;
    [Space(5)]

    [Header("Health")]
    [SerializeField] private float maxHealth = 100f;
    [SerializeField] private float currentHealth;
    [Space(5)]

    [Header("Armor")]
    [Tooltip("The armor class of this part. Determines how much damage is reduced based on shell penetration.")]
    [SerializeField] private ArmorType armorType = ArmorType.LIGHT;
    [Tooltip("The durability of this part. Determines how much damage is absorbed by the part.")]
    [Range(0f, 100f)]
    [SerializeField] private float durability = 100f;
    [Space(5)]

    [Header("Destruction")]
    [Tooltip("If true, the part is destroyed (disabled) when HP reaches 0.")]
    [SerializeField] private bool isDestructible = true;

    [Tooltip("If true, destroying this part kills the whole enemy (MainFrame).")]
    [SerializeField] private bool isMainFrame = false;
    [Space(5)]

    [Header("Debug")]
    [SerializeField] private bool logDamage = false;

    // Events
    /// <summary>Fires on every hit with (damageTaken, currentHP, maxHP, isCrit, effectColor, isEffect)</summary>
    public event Action<float, float, float, bool, Color, bool> OnDamaged;

    /// <summary>Fires once when HP reaches 0.</summary>
    public event Action<HealthComponent> OnDestroyed;

    // Armor Reduction State
    private ArmorType baseArmorType;
    private float armorReductionTimer = 0f;

    // Cached reference
    private StatusEffectHandler handler;

    private void Awake()
    {
        currentHealth = maxHealth;
        this.baseArmorType = armorType;

        // Auto-add and init the StatusEffectHandler
        this.handler = gameObject.AddComponent<StatusEffectHandler>();
        this.handler.Init(this);
    }

    private void Update()
    {
        // Tick armor reduction timer
        if(armorReductionTimer > 0f)
        {
            armorReductionTimer -= Time.deltaTime;
            if(armorReductionTimer < 0f )
            {
                armorType = baseArmorType; // restore
            }
        }
    }

    // IDamageable implementation
    public void HandleHit(Shell shell, RaycastHit hit)
    {
        // Ignore friendly fire
        if (shell.GetTeam() == team) return;

        float damage = shell.GetFinalDamage(armorType, this);
        TakeDamage(damage, shell.GetEffectColor(), shell.IsCrit());

        // Immediate effects (Explosion, Impact) - Handled directly in Shell
        shell.ApplySplecialEffect(this, hit);

        // DoT / debuff effects - handled by StatusEffectHandler
        if(shell.GetTypeEffect() != TypeEffect.NONE && 
           shell.GetTypeEffect() != TypeEffect.EXPLOSION &&
           shell.GetTypeEffect() != TypeEffect.IMPACT)
        {
            handler?.Apply(shell.GetTypeEffect(), shell.GetTeam(), shell.GetEffectData());
        }

        // LASER effect : only apply if weapon is laser type
        if(shell.GetTypeEffect() == TypeEffect.LASER &&
           shell.GetWeaponType() != WeaponType.LASER)
        {
            return; // Block if weapon isn't actually a laser
        }

        if(shell.GetTeam() == Team.Player)
        {
            // Optionally, add feedback for the enemy here (e.g., hit sparks, sound effects).
            // Damage popups

            // DamagePopupManager.instance.Show(damage, shell.IsCrit(), transform.position);
        }
    }

    public void TakeDamage(float damage, Color effectColor, bool isEffect, bool isCrit = false)
    {
        if(currentHealth <= 0) return; // Already destroyed

        float actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;

        if(logDamage)
        {
            Debug.Log($"[HealthComponent] {gameObject.name} took {actualDamage:0.#} damage." +
                $" {currentHealth:0.#}/{maxHealth} HP");
        }

        OnDamaged?.Invoke(actualDamage, currentHealth, maxHealth, isCrit, effectColor, isEffect);

        if(currentHealth <= 0)
        {
            Die();
        }
    }

    // Death
    private void Die()
    {
        currentHealth = 0f;

        OnDestroyed?.Invoke(this);

        if(isDestructible)
        {
            gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Reduces armor class by 'tiers' for 'duration' seconds.
    /// Armor is clamped to UNAMORED_I minimum.
    /// </summary>
    public void ApplyArmorReduction(int tiers, float duration)
    {
        int reduced = Mathf.Max(0, (int)baseArmorType - tiers);
        armorType   = (ArmorType)reduced;
        armorReductionTimer = Mathf.Max(armorReductionTimer, duration);

        Debug.Log($"[HealthComponent] {gameObject.name} armor reduced: {baseArmorType} -> {armorType} for {duration:0.0}s");
    }

    // Healing
    public void Heal(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
    }

    #region GETTERS
    public float GetCurrentHealth() { return currentHealth; }
    public float GetMaxHealth() { return maxHealth; }
    public float GetHealthRatio() { return currentHealth / maxHealth; }
    public ArmorType GetArmorType() { return armorType; }
    public Team GetTeam() { return team; }
    public bool IsMainFrame() { return isMainFrame; }
    public bool IsDead() { return currentHealth <= 0; }
    public float GetDurability() { return durability; }
    #endregion
}
