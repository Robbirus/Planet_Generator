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

    [Header("Slider")]
    [SerializeField] private Slider loadingSlider;
    [Space(5)]

    [Header("Progress")]
    [SerializeField] private TMP_Text progressTextValue;

    public void LoadLevel(int sceneIndex)
    {
        StartCoroutine(LoadLevelAsync(sceneIndex));
    }

    IEnumerator LoadLevelAsync(int levelToLoad)
    {
        AsyncOperation loadOperation = SceneManager.LoadSceneAsync(levelToLoad);
        loadingScreen.SetActive(true);

        MusicManager.instance.Stop();
        
        while (!loadOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(loadOperation.progress / 0.9f);

            loadingSlider.value = progressValue;
            progressTextValue.text = progressValue * 100f + "%";

            yield return null;
        }

        switch (levelToLoad)
        {
            case (int)SceneIndex.MENU:
                GameManager.instance.ChangeState(GameState.Menu);
                break;
            case (int)SceneIndex.GAME:
                GameManager.instance.ChangeState(GameState.Playing);
                break;
        }
    }
}
