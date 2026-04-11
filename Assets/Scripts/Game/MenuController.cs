using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.Audio;

public class MenuController : MonoBehaviour
{
    [Header("Level Loader Script")]
    [SerializeField] private LoadingController loadingController;
    [Space(5)]

    [Header("Volume Setting")]
    [SerializeField] private AudioMixer audioMixer;
    [Space(5)]

    [Header("Confirmation Image")]
    [SerializeField] private GameObject confirmationPrompt = null;
    [Space(10)]

    [Header("Levels to Load")]
    [SerializeField] private string newGameLevel;
    [Space(10)]

    private string levelToLoad;

    private void Start()
    {
        
    }

    #region Dialog Methods
    public void NewGameDialogYes()
    {
        loadingController.ApplyGame();
    }

    public void ExitButton()
    {
        Application.Quit();
    }
    #endregion

    /// <summary>
    /// Show an image to confirm action
    /// </summary>
    /// <returns></returns>
    public IEnumerator ConfirmationBox()
    {
        confirmationPrompt.SetActive(true);
        yield return new WaitForSeconds(2);
        confirmationPrompt.SetActive(false);
    }
}
