using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBarUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Slider fillImage;

    [Header("Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 2f, 0f);
    [SerializeField] private Color colorFull = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color colorDanger = new Color(0.9f, 0.2f, 0.2f);
    [Tooltip("Bar disappears when HP is full (saves screen clutter).")]
    [SerializeField] private bool hideWhenFull = true;

    private HealthComponent tracked;
    private Transform cam;

    // Init

    /// <summary>Binds this bar to a HealthComponent.</summary>
    public void Init(HealthComponent hc)
    {
        tracked = hc;
        cam = Camera.main.transform;

        hc.OnDamaged += OnDamaged;
        hc.OnDestroyed += OnDestroyed;

        UpdateBar(hc.GetHealthRatio());
    }


    private void LateUpdate()
    {
        if (tracked == null) { Destroy(gameObject); return; }

        // Follow the part
        transform.position = tracked.transform.position + offset;
        transform.localScale = new Vector3(0.3f, 0.3f, 0.3f); // Keep a consistent size regardless of distance

        // Billboard — always face the camera
        transform.LookAt(transform.position + cam.forward);
    }

    private void OnDestroy()
    {
        if (tracked == null) return;
        tracked.OnDamaged -= OnDamaged;
        tracked.OnDestroyed -= OnDestroyed;
    }

    // Handlers

    private void OnDamaged(float damage, float current, float max)
    {
        float ratio = max > 0f ? current / max : 0f;
        UpdateBar(ratio);
    }

    private void OnDestroyed(HealthComponent hc)
    {
        Destroy(gameObject);
    }

    // Visuals

    private void UpdateBar(float ratio)
    {
        if (fillImage == null) return;

        fillImage.value = ratio;
        fillImage.fillRect.GetComponent<Image>().color = Color.Lerp(colorDanger, colorFull, ratio);

        if (hideWhenFull)
            gameObject.SetActive(ratio < 0.999f);
    }
}