using UnityEngine;
using TMPro;

public class StellarLabel : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private bool isMoon;
    [SerializeField] private float showMoonThreshold = 40000f;
    [Tooltip("Vertical offset in pixels above the planet dot.")]
    [SerializeField] private float verticalOffset = 500f;
    [SerializeField] private float solarScale = 0.1f;
    [SerializeField] private float fontSize = 30f;

    private TextMeshProUGUI     labelInstance;
    private Camera              mapCamera;
    private StellarMapManager   stellarMapManager;
    private Canvas              worldCanvas;
    private bool                isMapOpen = false;

    public void Setup(TextMeshProUGUI prefab, Camera cam, StellarMapManager stellarMapManager)
    {
        if(prefab == null)
        {
            Debug.LogError("[StellarLabel] prefab is null.", this);
            return;
        }

        // Idempotency guard: if Setup() is called again on an already-initialized label
        // (e.g. RefreshAllLabels() called twice), unsubscribe the old event handler and
        // destroy the previous canvas instead of leaking a duplicate.
        if (this.stellarMapManager != null)
        {
            this.stellarMapManager.OnMapChanged -= OnMapChanged;
        }
        if (worldCanvas != null)
        {
            Destroy(worldCanvas.gameObject);
        }

        this.mapCamera          = cam;
        this.stellarMapManager  = stellarMapManager;

        // Create the canvas
        GameObject canvasGO = new GameObject("Label Canvas");
        canvasGO.transform.SetParent(transform, false);
        canvasGO.transform.localPosition = Vector3.zero;

        worldCanvas             = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode  = RenderMode.WorldSpace;
        worldCanvas.worldCamera = cam;

        // Canvas size in world unit
        RectTransform rectTransform = canvasGO.GetComponent<RectTransform>();
        rectTransform.sizeDelta = new Vector2(2000f, 500f);
        rectTransform.localScale = Vector3.one * solarScale;

        // Instantiate the label in the container
        labelInstance           = Instantiate(prefab, canvasGO.transform);
        labelInstance.text      = gameObject.name;
        labelInstance.fontSize  = fontSize;
        labelInstance.GetComponent<RectTransform>().anchoredPosition = new Vector2(0f, verticalOffset);
        labelInstance.gameObject.SetActive(false);


        // Subscribe  
        if (this.stellarMapManager != null)
        {
            stellarMapManager.OnMapChanged += OnMapChanged;
        }
    }

    private void LateUpdate()
    {
        if (labelInstance == null || !isMapOpen) return;
        if (mapCamera == null || !mapCamera.gameObject.activeInHierarchy) return;

        // Display logic for moons
        if (isMoon)
        {
            bool closeEnough = mapCamera.orthographicSize < showMoonThreshold;
            labelInstance.gameObject.SetActive(closeEnough);
            if (!closeEnough) return;
        }

        // Billboard
        worldCanvas.transform.LookAt(
            worldCanvas.transform.position + mapCamera.transform.rotation * Vector3.forward,
            mapCamera.transform.rotation * Vector3.up);

    }

    private void OnMapChanged(bool isMapOpen)
    {
        this.isMapOpen = isMapOpen;

        if (this.labelInstance == null) return;

        if(!this.isMapOpen)
        {
            labelInstance.gameObject.SetActive(false);
            return;
        }

        if (!isMoon)
        {
            labelInstance.gameObject.SetActive(true);
        }
    }

    private void OnDestroy()
    {
        if(this.stellarMapManager != null)
        {
            this.stellarMapManager.OnMapChanged -= OnMapChanged;
        }

        if(labelInstance != null)
        {
            Destroy(labelInstance.gameObject);
        }
    }
}