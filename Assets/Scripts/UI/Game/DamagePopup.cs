using System.Collections;
using TMPro;
using UnityEngine;

public class DamagePopup : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private TMP_Text label;

    [Header("Animation")]
    [Tooltip("Total lifetime of the popup in seconds")]
    [SerializeField] private float lifetime = 1.2f;
    [Tooltip("Upward speed in pixels per second")]
    [SerializeField] private float riseSpeed = 80f;
    [Space(5)]

    [Header("Style")]
    [SerializeField] private Color colorNormal = Color.white;
    [SerializeField] private Color colorCrit = new Color(1f, 0.7f, 0f);
    [SerializeField] private float normalSize = 28f; // TMP font size
    [SerializeField] private float critSize = 38f;
    [SerializeField] private float scale = 1f; // Scale multiplier for font size
    [Space(5)]

    [Header("Scatter")]
    [Tooltip("Max horizontal scatter in pixels so stacked popups don't overlap")]
    [SerializeField] private float scatterX = 30f;
    [Tooltip("Max vertical scatter in pixels")]
    [SerializeField] private float scatterY = 15f;
    [Tooltip("Scale multiplier for the X axis")]
    [SerializeField] private float scatterScaleX = 50f;
    [Tooltip("Scale multiplier for the Y axis")]
    [SerializeField] private float scatterScaleY = 50f;
    [Space(5)]

    [Header("Debug")]
    [Tooltip("If true, popups will have a random scatter offset to avoid stacking on top of each other.")]
    [SerializeField] private bool isScattered = false;
    [Tooltip("If true, logs to console.")]
    [SerializeField] private bool isDebugMode = false;

    private RectTransform rect;
    private System.Random rng;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        rng = SeedManager.GetRNG("damagePopup");
    }

    /// <summary>Call immediately after instantiation.</summary>
    public void Init(float damage, bool isCrit, Vector3 screenPos, Color effectColor, bool isEffect = false)
    {
        // Texte
        if(damage < 0.01f)
        {
            label.text = "";
        }
        else
        {
            label.text = $"{damage: 0}";
        }

        if (isEffect)
        {
            label.color = effectColor;
            label.fontSize = normalSize * scale;
        }
        else
        {
            label.color = isCrit ? colorCrit : colorNormal;
            label.fontSize = (isCrit ? critSize : normalSize) * scale;
        }


        // Initial position
        rect.anchoredPosition = new Vector3(0f, 0f, 0f); // Start at center, then apply scatter and movement

        // Small variation
        if(isScattered)
        {
            rect.anchoredPosition += new Vector2(
                SeedManager.Range(-scatterX, scatterX, rng) * scatterScaleX,
                SeedManager.Range(0f, scatterY, rng) * scatterScaleY
            );
        }

        if (isDebugMode)
        {
            Debug.Log($"DamagePopup Init: damage={damage:0}, position={rect.anchoredPosition}");
        }

        // Rotation
        rect.rotation = Quaternion.identity;

        // Scale "pop"
        StartCoroutine(ScaleUp());

        // Destruction
        Destroy(gameObject, lifetime);
    }

    private void Update()
    {
        // Rises upwards
        rect.anchoredPosition += Vector2.up * riseSpeed * Time.deltaTime;

        // Fade out
        FadeOut();
    }

    private void FadeOut()
    {
        float alpha = Time.deltaTime / lifetime;

        if (label != null)
        {
            Color c = label.color;
            c.a -= alpha;
            label.color = c;
        }
    }

    private IEnumerator ScaleUp()
    {
        rect.localScale = Vector3.zero;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime * 8f;
            rect.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }
    }
}