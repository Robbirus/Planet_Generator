using UnityEngine;


public class EnemyHealthBarManager : MonoBehaviour
{
    [Header("Prefab")]
    [Tooltip("WorldSpaceHealthBar prefab, one instance will be spawned per part.")]
    [SerializeField] private EnemyHealthBarUI healthBarPrefab;
    [Space(5)]

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

            part.OnDamaged += (damage, currentHP, maxHP, isCrit, effectColor, isEffect) =>
            {
                DamagePopupManager.instance?.Show(damage, isCrit, part.transform.position, effectColor, isEffect);
            };
        }
    }
}