using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Orchestrates the health of a multi-part enemy.
/// The enemy dies ONLY when its MainFrame HealthComponent is destroyed.
/// All other parts are optional destructible pieces (wings, engines, turretsÅc).
/// </summary>
public class EnemyHealth : MonoBehaviour
{
    [Header("Main Frame")]
    [Tooltip("The critical part - destroying this kills the enemy.")]
    [SerializeField] private HealthComponent mainFrame;
    [Space(5)]

    [Header("All Parts (auto-discovered if empty)")]
    [SerializeField] private List<HealthComponent> parts = new();
    [Space(5)]

    [Header("Debug")]
    [SerializeField] private bool logEvents = false;

    // Events
    /// <summary>Fires when any part is destroyed, with the destroyed part.</summary>
    public event Action<HealthComponent> OnPartDestroyed;

    /// <summary>Fires when the MainFrame is destroyed (= enemy death).</summary>
    public event Action<EnemyHealth> OnEnemyDeath;

    private bool isDead = false;

    private void Awake()
    {
        // Auto-discover all HealthComponents in children if none were assigned
        if(parts.Count == 0)
        {
            parts.AddRange(GetComponentsInChildren<HealthComponent>());
        }

        // Auto-Assign MainFrame from parts if not set
        if(mainFrame == null)
        {
            foreach(HealthComponent part in parts)
            {
                if(part.IsMainFrame())
                {
                    mainFrame = part;
                    break;
                }
            }
        }

        // Check if MainFrame was found
        if(mainFrame == null)
        {
            Debug.LogError($"[EnemyHealth] No MainFrame HealthComponent found. " +
                "Tick 'Is Main Frame' on the critical part.", this);
        }

        // Subscribe to all parts' OnDestroyed events
        foreach(HealthComponent part in parts)
        {
            part.OnDestroyed += HandlePartDestroyed;
        }
    }

    private void OnDestroy()
    {
        foreach(HealthComponent part in parts)
        {
            if(part != null) part.OnDestroyed -= HandlePartDestroyed;
        }
    }

    // Handler
    private void HandlePartDestroyed(HealthComponent destroyedPart)
    {
        if (logEvents)
        {
            Debug.Log($"[EnemyHealth] Part destroyed: {destroyedPart.gameObject.name}", this);
        }

        OnPartDestroyed?.Invoke(destroyedPart);

        // Enemy only dies when the MainFrame is the destroyed part
        if (destroyedPart == mainFrame)
        {
            Die();
        }
    }

    private void Die()
    {
        if (isDead) return;
        isDead = true;

        if(logEvents)
        {
            Debug.Log($"[EnemyHealth] {gameObject.name} DESTROYED (MainFrame lost).", this);
        }

        OnEnemyDeath?.Invoke(this);

        // Disable all remaining active parts
        foreach(HealthComponent part in parts)
        {
            if(part != null && part.gameObject.activeSelf)
            {
                part.gameObject.SetActive(false);
            }
        }

        Destroy(gameObject, 0.1f);
    }

    // GETTERS
    public bool IsDead() { return isDead; }
    public HealthComponent GetMainFrame() { return mainFrame; }
    public List<HealthComponent> GetAllParts() { return parts; }

    ///<summary>Returns the total HP ratio across all living parts (0-1).</summary>
    public float GetOverallHealthRatio()
    {
        float total = 0f;
        float current = 0f;

        foreach(HealthComponent part in parts)
        {
            total += part.GetMaxHealth();
            current += part.GetCurrentHealth();
        }

        return total > 0f ? current / total : 0f;
    }
}
