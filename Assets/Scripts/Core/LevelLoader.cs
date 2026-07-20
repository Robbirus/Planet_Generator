using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    [Header("Screen")]
    [SerializeField] private GameObject loadingScreen;
    [Space(5)]

    [Header("Loading Progress Bar")]
    [SerializeField] private Slider loadingSlider;
    [SerializeField] private TMP_Text progressTextValue;
    [Space(5)]

    [Header("Distribution of the bar")]
    [Tooltip("Fraction of the bar reserved for loading Unity assets (0-1).\n" +
             "The rest is used for procedural generation.\n" +
             "Example: 0.8 = 0-80% assets, 80-100% generation.")]
    [SerializeField, Range(0.5f, 0.95f)] private float assetLoadingShare = 0.8f;
    [Space(5)]

    [Header("Smoothing")]
    [SerializeField, Tooltip("Speed at which the visual progress bar catches up to the actual progress.")] private float visualProgressSpeed = 0.5f;
    private float targetProgress = 0f;
    private float currentVisualProgress = 0f;

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadLevelAsync(sceneIndex));
    }

    IEnumerator LoadLevelAsync(int levelToLoad)
    {
        loadingScreen.SetActive(true);
        UpdateUI(0f);

        // Initialization of smoothing variables at the start
        currentVisualProgress = 0f;
        targetProgress = 0f;

        bool needsGenerationWait = levelToLoad == (int)SceneIndex.GAME;

        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadOperation.allowSceneActivation = false;

        // STEP 1 : Loading Scene Assets
        while (loadOperation.progress < 0.9f)
        {
            // If the next scene does not need generation, assets represent 100% (1.0f) of the load
            float share = needsGenerationWait ? assetLoadingShare : 1f;
            targetProgress = (loadOperation.progress / 0.9f) * share;

            // Smoothly move the visual bar toward this target.
            currentVisualProgress = Mathf.MoveTowards(currentVisualProgress, targetProgress, Time.deltaTime * visualProgressSpeed);
            UpdateUI(currentVisualProgress);

            yield return null;
        }

        targetProgress = needsGenerationWait ? assetLoadingShare : 1f;

        // Wait for the visual bar to catch up with the target before activating the scene.
        while (currentVisualProgress < targetProgress)
        {
            currentVisualProgress = Mathf.MoveTowards(currentVisualProgress, targetProgress, Time.deltaTime * visualProgressSpeed);
            UpdateUI(currentVisualProgress);
            yield return null;
        }

        loadOperation.allowSceneActivation = true;

        // STEP 2: Procedural generation (Only if required)
        if (needsGenerationWait)
        {
            // Waiting for the operation to be completely completed (effective scene change)
            while (!loadOperation.isDone)
            {
                yield return null;
            }

            // Calculation of the remaining space on the bar
            float generationShare = 1f - assetLoadingShare;

            // Loop as long as the generation isn’t marked complete.
            while (!SolarSystemGenerator.IsGenerationComplete)
            {
                // Get the static progress 
                float rawGenProgress = SolarSystemGenerator.GenerationProgress;

                // Normalize progression generation
                float normalizedGenProgress = Mathf.Clamp01(rawGenProgress / 0.9f);

                // The new target is: the base + the progress of the generator on the remaining space
                targetProgress = assetLoadingShare + (normalizedGenProgress * generationShare);

                // Lerp Smoothing Visual Progression
                currentVisualProgress = Mathf.MoveTowards(currentVisualProgress, targetProgress, Time.deltaTime * visualProgressSpeed);
                UpdateUI(currentVisualProgress);

                yield return null;
            }
        }

        // FINAL STEP: Filling at 100%
        targetProgress = 1f;
        while (currentVisualProgress < 1f)
        {
            currentVisualProgress = Mathf.MoveTowards(currentVisualProgress, targetProgress, Time.deltaTime * visualProgressSpeed);
            UpdateUI(currentVisualProgress);
            yield return null;
        }

        yield return new WaitForSeconds(0.2f);

        switch (levelToLoad)
        {
            case (int)SceneIndex.GAME:
                GameManager.instance.ChangeState(GameState.Playing);
                break;
            case (int)SceneIndex.MENU:
                GameManager.instance.ChangeState(GameState.Menu);
                break;
        }

        loadingScreen.SetActive(false);
    }
    
    /// <summary>Updates the percentage bar and text.</summary>
    private void UpdateUI(float progress01)
    {
        if (loadingSlider != null) loadingSlider.value = progress01;
        if (progressTextValue != null) progressTextValue.text =
            Mathf.RoundToInt(progress01 * 100f) + "%";
    }

}
