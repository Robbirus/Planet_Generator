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
    /// <summary>Fires on every hit with (damageTaken, currentHP, maxHP)</summary>
    public event Action<float, float, float> OnDamaged;

    /// <summary>Fires once when HP reaches 0.</summary>
    public event Action<HealthComponent> OnDestroyed;

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    // IDamageable implementation
    public void HandleHit(Shell shell, RaycastHit hit)
    {
        // Ignore friendly fire
        if (shell.GetTeam() == team) return;

        float damage = shell.GetFinalDamage(armorType, this);

        TakeDamage(damage);

        // Apply status effects
        if (shell.GetTypeEffect() != TypeEffect.NONE)
        {
            ApplyEffect(shell.GetTypeEffect());
        }

        if(shell.GetTeam() == Team.Player)
        {
            // Optionally, add feedback for the enemy here (e.g., hit sparks, sound effects).
            // Damage popups
            DamagePopupManager.instance.Show(damage, shell.IsCrit(), transform.position);
        }
    }

    public void TakeDamage(float damage)
    {
        if(currentHealth <= 0) return; // Already destroyed

        float actualDamage = Mathf.Min(damage, currentHealth);
        currentHealth -= actualDamage;

        if(logDamage)
        {
            Debug.Log($"[HealthComponent] {gameObject.name} took {actualDamage:0.#} damage." +
                $" {currentHealth:0.#}/{maxHealth} HP");
        }

        OnDamaged?.Invoke(actualDamage, currentHealth, maxHealth);

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

    // Effects
    private void ApplyEffect(TypeEffect effect)
    {
        // Placeholder for applying status effects like fire, EMP, etc.
        // This could involve adding components, starting coroutines, etc.
        Debug.Log($"[HealthComponent] {gameObject.name} affected by {effect}");
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
