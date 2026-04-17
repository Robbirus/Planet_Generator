using UnityEngine;


public class EnemyHealthBarManager : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("WorldSpaceHealthBar prefab — one instance will be spawned per part.")]
    [SerializeField] private EnemyHealthBarUI healthBarPrefab;

    [Header("Parent (optional)")]
    [Tooltip("If assigned, all bars are parented here. Leave empty to use scene root.")]
    [SerializeField] private Transform barParent;

    private void Start()
    {
        if (healthBarPrefab == null)
        {
            Debug.LogWarning("[EnemyHealthBarManager] healthBarPrefab is not assigned.", this);
            return;
        }

        // Spawn one bar for every HealthComponent in the children
        HealthComponent[] parts = GetComponentsInChildren<HealthComponent>();

        foreach (HealthComponent part in parts)
        {
            EnemyHealthBarUI bar = Instantiate(
                healthBarPrefab,
                part.transform.position,
                Quaternion.identity,
                barParent);

            bar.Init(part);

            part.OnDamaged += (damage, current, max) =>
                DamagePopupManager.instance.Show(
                damage,
                false,
                part.transform.position);

            Debug.Log($"[EnemyHealthBarManager] Spawned health bar for '{part.gameObject.name}'.");
        }
    }
}