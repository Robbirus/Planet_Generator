using UnityEngine;

/// <summary>
/// Spawns DamagePopup instances on a Screen Space canvas.
/// Converts world position -> screen position and scales the popup distance
/// so far-away numbers appear smaller than close ones.
/// </summary>
public class DamagePopupManager : MonoBehaviour
{
    public static DamagePopupManager instance = null;

    [Header("Prefab")]
    [Tooltip("DamagePopup prefab. Must have a DamagePopup component on the root.")]
    [SerializeField] private DamagePopup popupPrefab;
    [Space(5)]

    [Header("Canvas")]
    [Tooltip("Screen Space Canvas where popup are spawned. Must be Screen Overlay.")]
    [SerializeField] private Canvas popupCanvas;
    [Space(5)]

    [Header("Debug")]
    [Tooltip("Log damage popup spawns to console.")]
    [SerializeField] private bool logPopups = false;

    private Camera cam;

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
            cam = Camera.main;
        }
    }

    /// <summary>
    /// Converts worldPosition to screen space and spawns a popup on the UI canvas.
    /// Call this from EnemyHealthBarManager or HealthComponent events.
    /// </summary>
    public void Show(float damage, bool isCrit, Vector3 worldPosition, Color effectColor, bool isEffect)
    {
        if (popupPrefab == null)
        {
            Debug.LogWarning("[DamagePopupManager] popupPrefab is not assigned.", this);
            return;
        }

        if(popupCanvas == null)
        {
            Debug.LogWarning("[DamagePopupManager] popupCanvas is not assigned.", this);
            return;
        }

        // Up-shifts the object
        worldPosition += Vector3.up * 1.5f;

        Vector3 screenPos = cam.WorldToScreenPoint(worldPosition);

        // If behind the camera -> ignores
        if (screenPos.z < 0)
            return;

        // Convert to UI position
        RectTransform canvasRect = popupCanvas.GetComponent<RectTransform>();

        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            null,
            out anchoredPos
        );

        if (logPopups)
        {
            Debug.Log($"[DamagePopupManager] Showing damage popup: {damage:0} at {screenPos} (crit: {isCrit})");
        }

        // Spawn
        DamagePopup popup = Instantiate(popupPrefab, popupCanvas.transform);
        popup.Init(damage, isCrit, anchoredPos, effectColor, isEffect);
    }
}