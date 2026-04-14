using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


[RequireComponent(typeof(RectTransform))]
public class InfiniteCanvas : MonoBehaviour, IDragHandler, IScrollHandler, IPointerDownHandler
{
    // ── Inspector ──────────────────────────────────────────────────────────────

    [Header("References")]
    [Tooltip("The outer container with the Mask. Pan is constrained inside this.")]
    [SerializeField] private RectTransform containerRect;

    [Header("Zoom")]
    [SerializeField] private float minZoom = 0.3f;
    [SerializeField] private float maxZoom = 2.0f;
    [SerializeField] private float zoomSpeed = 0.1f;
    [SerializeField] private float zoomSmoothing = 8f;

    [Header("Pan")]
    [Tooltip("Multiplier applied to drag delta. 1 = 1:1 with cursor.")]
    [SerializeField] private float panSpeed = 1f;

    [Header("Constraints")]
    [Tooltip("Limit panning so content cannot be dragged fully out of view.")]
    [SerializeField] private bool constrainPan = true;

    [Header("Start Position")]
    [Tooltip("World-space position offset of the canvas at start (0,0 = centered).")]
    [SerializeField] private Vector2 startOffset = Vector2.zero;

    // ── Internal ───────────────────────────────────────────────────────────────

    private RectTransform contentRect;
    private float currentZoom;
    private float targetZoom;
    private Vector2 targetPosition;

    // ───────────────────────────────────────────────────────────────────────────

    private void Awake()
    {
        contentRect = GetComponent<RectTransform>();

        currentZoom = 1f;
        targetZoom = 1f;
        targetPosition = startOffset;

        contentRect.localScale = Vector3.one * currentZoom;
        contentRect.anchoredPosition = targetPosition;
    }

    private void Update()
    {
        // Smooth zoom interpolation
        if (!Mathf.Approximately(currentZoom, targetZoom))
        {
            currentZoom = Mathf.Lerp(currentZoom, targetZoom, zoomSmoothing * Time.unscaledDeltaTime);
            contentRect.localScale = Vector3.one * currentZoom;
        }

        // Smooth pan interpolation
        contentRect.anchoredPosition = Vector2.Lerp(
            contentRect.anchoredPosition,
            targetPosition,
            zoomSmoothing * Time.unscaledDeltaTime
        );

        if (constrainPan)
            ApplyPanConstraints();
    }

    // ── IDragHandler ───────────────────────────────────────────────────────────

    public void OnDrag(PointerEventData eventData)
    {
        targetPosition += eventData.delta * panSpeed / currentZoom;
    }

    // ── IScrollHandler (zoom) ─────────────────────────────────────────────────

    public void OnScroll(PointerEventData eventData)
    {
        float scroll = eventData.scrollDelta.y;
        if (Mathf.Approximately(scroll, 0f)) return;

        float previousZoom = targetZoom;
        targetZoom = Mathf.Clamp(targetZoom + scroll * zoomSpeed, minZoom, maxZoom);

        // Zoom toward the mouse cursor position, not the canvas center
        if (containerRect != null &&
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                containerRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 localCursor))
        {
            float zoomDelta = targetZoom / previousZoom;
            // Shift the content so the point under the cursor stays fixed
            targetPosition = localCursor + (targetPosition - localCursor) * zoomDelta;
        }
    }

    // ── IPointerDownHandler ───────────────────────────────────────────────────

    // Required so drag events are properly received on the canvas
    public void OnPointerDown(PointerEventData eventData) { }

    // ── Pan Constraints ───────────────────────────────────────────────────────

    /// <summary>
    /// Prevents panning so far that the content rect fully leaves the container.
    /// Uses half the content size (scaled) as the boundary.
    /// </summary>
    private void ApplyPanConstraints()
    {
        if (containerRect == null) return;

        Vector2 containerSize = containerRect.rect.size;
        Vector2 contentSize = contentRect.rect.size * currentZoom;

        // Allow panning up to half the content size beyond the container center
        float maxX = Mathf.Max(0f, (contentSize.x - containerSize.x) * 0.5f);
        float maxY = Mathf.Max(0f, (contentSize.y - containerSize.y) * 0.5f);

        Vector2 pos = contentRect.anchoredPosition;
        pos.x = Mathf.Clamp(pos.x, -maxX, maxX);
        pos.y = Mathf.Clamp(pos.y, -maxY, maxY);

        contentRect.anchoredPosition = pos;
        targetPosition = pos; // sync so Lerp doesn't fight constraints
    }

    // ── Public API ────────────────────────────────────────────────────────────

    /// <summary>
    /// Resets the view to zoom=1 centered at the origin.
    /// Call this when opening the upgrade panel.
    /// </summary>
    public void ResetView()
    {
        targetZoom = 1f;
        targetPosition = startOffset;
    }

    /// <summary>
    /// Centers the view on a specific node (its RectTransform).
    /// Useful to highlight a newly unlocked skill.
    /// </summary>
    public void FocusOn(RectTransform node)
    {
        if (node == null) return;
        // The node's anchored position in the content is the vector to invert
        targetPosition = -node.anchoredPosition * targetZoom;
    }

}