using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using static UnityEditorInternal.ReorderableList;

public class MenuController : MonoBehaviour
{
    [Header("Level Loader Script")]
    [SerializeField] private LoadingController loadingController;
    [Space(5)]

    [Header("Volume Setting")]
    [SerializeField] private AudioMixer audioMixer;
    [Space(5)]

    [SerializeField] private TMP_Text bgmTextValue = null;
    [SerializeField] private int defaultBGM = 5;
    [Space(5)]

    [SerializeField] private TMP_Text sfxTextValue = null;
    [SerializeField] private int defaultSFX = 5;
    [Space(5)]

    public static int bgmVolume;
    public static int sfxVolume;

    [Header("Graphics Setting")]
    [SerializeField] private Slider brightnessSlider = null;
    [SerializeField] private TMP_Text brightnessTextValue = null;
    [SerializeField] private float defaultBrightness = 1f;
    [Space(5)]

    [SerializeField] private TMP_Dropdown qualityDropdown;
    [SerializeField] private Toggle fullScreenToggle;
    [Space(10)]

    private int qualityLevel;
    private bool isFullScreen;
    private float brightnessLevel;

    [Header("Confirmation Image")]
    [SerializeField] private GameObject confirmationPrompt = null;
    [Space(10)]

    [Header("Levels to Load")]
    [SerializeField] private string newGameLevel;
    [Space(10)]

    private string levelToLoad;

    private void Start()
    {
        LoadSoundPreference();
    }

    private void LoadSoundPreference()
    {
        if (PlayerPrefs.HasKey("BGMVolume"))
        {
            int localVolume = PlayerPrefs.GetInt("BGMVolume");
            MenuController.bgmVolume = localVolume;
            bgmTextValue.text = localVolume.ToString("0");
            audioMixer.SetFloat("BGM", Mathf.Log10(localVolume / 10f) * 20);
        }
        else
        {
            ResetButton("Audio");
        }

        if (PlayerPrefs.HasKey("SFXVolume"))
        {
            int localVolume = PlayerPrefs.GetInt("SFXVolume");
            MenuController.sfxVolume = localVolume;
            sfxTextValue.text = localVolume.ToString("0");
            audioMixer.SetFloat("SFX", Mathf.Log10(localVolume / 10f) * 20);
        }
        else
        {
            ResetButton("Audio");
        }
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

    public void ResetButton(string MenuType)
    {
        switch (MenuType)
        {
            case "Audio":

                audioMixer.SetFloat("BGM", Mathf.Log10(defaultBGM / 10f) * 20);
                bgmTextValue.text = defaultBGM.ToString("0");

                audioMixer.SetFloat("SFX", Mathf.Log10(defaultSFX / 10f) * 20);
                sfxTextValue.text = defaultSFX.ToString("0");


                ApplyVolume();
                break;

            case "Gameplay":
                break;

            case "Controls":
                break;

            case "Graphics":                
                // Reset brightness value
                brightnessSlider.value = defaultBrightness;
                brightnessTextValue.text = defaultBrightness.ToString("0.0");

                // Reset quality value
                qualityDropdown.value = 1;
                QualitySettings.SetQualityLevel(1);

                // Reset fullscreen 
                fullScreenToggle.isOn = false;
                Screen.fullScreen = false;

                // Reset Resolution
                Resolution currentResolution = Screen.currentResolution;
                Screen.SetResolution(currentResolution.width, currentResolution.height, Screen.fullScreen);
                // resolutionDropDown.value = resolutions.Length;
                ApplyGraphics();
                break;
        }
    }
    #endregion

    #region Audio Setting Methods
    public void ApplyVolume()
    {
        PlayerPrefs.SetInt("BGMVolume", bgmVolume);
        PlayerPrefs.SetInt("SFXVolume", sfxVolume);

        // Show Prompt
        StartCoroutine(ConfirmationBox());
    }

    public void IncreaseBGM()
    {
        if (bgmVolume < 10)
        {
            bgmVolume++;
        }
        audioMixer.SetFloat("BGM", Mathf.Log10(bgmVolume / 10f) * 20);
        bgmTextValue.text = bgmVolume.ToString("0");
    }

    public void DecreaseBGM()
    {
        if (bgmVolume > 0)
        {
            bgmVolume--;
        }

        if (bgmVolume == 0)
        {
            audioMixer.SetFloat("BGM", Mathf.Log10(-1 * 20));
        }
        else
        {
            audioMixer.SetFloat("BGM", Mathf.Log10(bgmVolume / 10f) * 20);
        }

        bgmTextValue.text = bgmVolume.ToString("0");
    }

    public void IncreaseSFX()
    {
        if (sfxVolume < 10)
        {
            sfxVolume++;
        }
        audioMixer.SetFloat("SFX", Mathf.Log10(sfxVolume / 10f) * 20);
        sfxTextValue.text = sfxVolume.ToString("0");
    }

    public void DecreaseSFX()
    {
        if (sfxVolume > 0)
        {
            sfxVolume--;
        }

        if (sfxVolume == 0)
        {
            audioMixer.SetFloat("SFX", Mathf.Log10(-1 * 20));
        }
        else
        {
            audioMixer.SetFloat("SFX", Mathf.Log10(sfxVolume / 10f) * 20);
        }

        sfxTextValue.text = sfxVolume.ToString("0");
    }
    #endregion

    #region Graphics Methods 
    public void SetBrightness(float brightness)
    {
        this.brightnessLevel = brightness;
        this.brightnessTextValue.text = brightness.ToString("0.0");
    }

    public void SetFullScreen(bool isFullScreen)
    {
        this.isFullScreen = isFullScreen;
    }

    public void SetQuality(int qualityIndex)
    {
        this.qualityLevel = qualityIndex;
    }

    public void SetResolution(int resolutionIndex)
    {
        /*
        Resolution resolution = resolutions[resolutionIndex];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
        */
    }

    public void ChangeResolution()
    {
        // SetResolution(resolutionDropDown.value);
    }

    /// <summary>
    /// Apply the graphics settings, with brightness, quality and/or fullscreen
    /// </summary>
    public void ApplyGraphics()
    {
        // Change your brightness with your post processing or whatever it is
        PlayerPrefs.SetFloat("Brightness", brightnessLevel);

        PlayerPrefs.SetInt("Quality", qualityLevel);
        QualitySettings.SetQualityLevel(qualityLevel);

        PlayerPrefs.SetInt("Fullscreen", (isFullScreen ? 1 : 0));
        Screen.fullScreen = isFullScreen;

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

