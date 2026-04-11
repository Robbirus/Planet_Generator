using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class MenuPause : MonoBehaviour
{
    [Header("Static Pause Value")]
    public static bool isGamePaused = false;
    [Space(10)]

    [Header("UI Containers")]
    [SerializeField] private GameObject pauseMenuContainer;
    [SerializeField] private GameObject hudContainer;
    [Space(10)]

    [Header("Level Loading")]
    [SerializeField] private LevelLoader levelLoader;
    [Space(10)]

    [Header("Input Action Reference")]
    [SerializeField] private InputActionReference pauseActionReference;
    [Space(10)]

    [Header("Confirmation Image")]
    [SerializeField] private GameObject confirmationPrompt = null;

    private void LoadSoundPreference()
    {
        throw new NotImplementedException();
    }

    private void OnEnable()
    {
        pauseActionReference.action.performed += OnPausePressed;
        pauseActionReference.action.Enable();
    }

    private void OnDisable()
    {
        pauseActionReference.action.performed -= OnPausePressed;
        pauseActionReference.action.Disable();
    }

    private void OnPausePressed(InputAction.CallbackContext context)
    {
        if (isGamePaused)
        {
            ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    public void ResumeGame()
    {
        pauseMenuContainer.SetActive(false);
        hudContainer.SetActive(true);

        Time.timeScale = 1f;
        isGamePaused = false;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = false;
    }

    private void PauseGame()
    {
        pauseMenuContainer.SetActive(true);
        hudContainer.SetActive(false);

        Time.timeScale = 0f;
        isGamePaused = true;

        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;
    }

    /// <summary>
    /// Load the Scene Menu
    /// </summary>
    public void LoadMenu()
    {
        isGamePaused = false;
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        levelLoader.LoadLevel((int)SceneIndex.MENU);
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    #region Audio Volume
    public void IncreaseBGM()
    {
        throw new NotImplementedException();
    }

    public void DecreaseBGM()
    {
        throw new NotImplementedException();
    }

    public void IncreaseSFX()
    {
        throw new NotImplementedException();
    }

    public void DecreaseSFX()
    {
        throw new NotImplementedException();
    }

    public void ApplyVolume()
    {
        // Show Prompt
        StartCoroutine(ConfirmationBox());
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
