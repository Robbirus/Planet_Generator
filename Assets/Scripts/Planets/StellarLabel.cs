using UnityEngine;
using TMPro;

public class StellarLabel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string displayName;
    [SerializeField] private bool isMoon;
    [SerializeField] private float showMoonThreshold = 40000f;

    private TextMeshProUGUI labelInstance;
    private Camera mapCamera;

    public void Setup(TextMeshProUGUI prefab, Transform canvas, Camera cam)
    {
        labelInstance = Instantiate(prefab, canvas);

        displayName = gameObject.name;
        labelInstance.text = displayName;

        mapCamera = cam;
    }

    private void Update()
    {
        if (labelInstance == null || !mapCamera.gameObject.activeInHierarchy) return;

        // Display logic for moons
        if (isMoon)
        {
            bool closeEnough = mapCamera.orthographicSize < showMoonThreshold;
            labelInstance.gameObject.SetActive(closeEnough);
        }

        // Positioning of the text relative to the screen
        Vector3 screenPos = mapCamera.WorldToScreenPoint(transform.position);
        labelInstance.transform.position = screenPos;
    }

    private void OnDestroy() { if (labelInstance) Destroy(labelInstance.gameObject); }
}