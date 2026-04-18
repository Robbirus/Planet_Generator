using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text label;

    [Header("Animation")]
    [SerializeField] private float lifetime = 1.2f;
    [SerializeField] private float riseSpeed = 1.5f;
    [SerializeField] private float fadeStart = 0.6f;  // ratio of lifetime when fade begins

    [Header("Style")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorCrit = new Color(1f, 0.7f, 0f);
    [SerializeField] private float normalSize = 0.4f;
    [SerializeField] private float critSize = 0.6f;
    [SerializeField] private Vector3 randomOffset = new Vector2(5f, 5f);
    [SerializeField] private Vector3 scale = new Vector2(0.3f, 0.3f);
    [SerializeField] private float scatterRadius = 100f;

    private float elapsed;
    private bool isCrit;
    private Transform cam;
    private System.Random rng;

    private void Awake()
    {
        rng = SeedManager.GetRNG("damagePopup");
    }

    /// <summary>Call immediately after instantiation.</summary>
    public void Init(float damage, bool isCrit, Vector3 worldPosition)
    {
        this.isCrit = isCrit;
        cam = Camera.main.transform;

        // Random horizontal scatter so multiple popups don't stack
        Vector3 scatter = new Vector3(
            SeedManager.Range(-randomOffset.x * scatterRadius, randomOffset.x * scatterRadius, rng),
            SeedManager.Range(0f, randomOffset.y * scatterRadius, rng),
            0f);

        transform.position = worldPosition + scatter;

        if (label != null)
        {
            label.rectTransform.localScale = scale;
            label.text = isCrit ? $"{damage:0.#}" : $"{damage:0.#}";
            label.color = isCrit ? colorCrit : colorNormal;
            label.fontSize = isCrit ? critSize : normalSize;
        }

        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        elapsed += Time.deltaTime;
        float t = Mathf.Clamp01(elapsed / lifetime);

        // Rise
        transform.position += Vector3.up * riseSpeed * Time.deltaTime;

        // Billboard
        if (cam != null)
            transform.LookAt(transform.position + cam.forward);

        // Fade out in the last portion of lifetime
        if (label != null && t >= fadeStart)
        {
            float alpha = 1f - Mathf.InverseLerp(fadeStart, 1f, t);
            Color c = label.color;
            c.a = alpha;
            label.color = c;
        }
    }
}