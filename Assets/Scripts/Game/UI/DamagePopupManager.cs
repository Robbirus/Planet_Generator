using UnityEngine;

public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager instance;

    [Header("Prefab")]
    [SerializeField] private DamagePopup popupPrefab;

    [Header("Parent (optional)")]
    [Tooltip("Parent transform for spawned popups. Leave empty for scene root.")]
    [SerializeField] private Transform popupParent;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        else
        {
            instance = this;
        }
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// Spawns a floating damage number at the given world position.
    /// Called from EnemyHealthBarManager (which bridges HealthComponent -> popup).
    /// </summary>
    public void Show(float damage, bool isCrit, Vector3 worldPosition)
    {
        if (popupPrefab == null)
        {
            Debug.LogWarning("[DamagePopupManager] popupPrefab is not assigned.", this);
            return;
        }

        Debug.Log($"[DamagePopupManager] Showing damage popup: {damage} at {worldPosition} (crit: {isCrit})");
        DamagePopup popup = Instantiate(popupPrefab, worldPosition, Quaternion.identity, popupParent);
        popup.Init(damage, isCrit, worldPosition);
    }
}