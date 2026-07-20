using UnityEngine;
using TMPro;

public class LoadingTextFade : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI loadingText;
    [SerializeField] private float fadeSpeed = 2f;
    [SerializeField] private float minAlpha = 0.2f;
    [SerializeField] private float maxAlpha = 1f;

    void Update()
    {
        if (loadingText == null) return;

        float alpha = Mathf.Lerp(minAlpha, maxAlpha, (Mathf.Sin(Time.time * fadeSpeed) + 1f) / 2f);

        Color newColor = loadingText.color;
        newColor.a = alpha;
        loadingText.color = newColor;
    }
}