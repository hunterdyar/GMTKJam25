using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private Toggle fullscreenToggle;
    [SerializeField] private TMP_Dropdown resolutionDropdown;
    [SerializeField] private Slider masterSlider;
    [SerializeField] private Slider musicSlider;
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private AudioMixer audioMixer;

    [SerializeField] private float defaultMasterVolume = 0.6f;
    [SerializeField] private float defaultMusicVolume = 0.6f;
    [SerializeField] private float defaultSFXVolume = 0.6f;

    private Resolution[] resolutions;

    private void Start()
    {
        LoadSettings(); // Loads and applies settings
    }

    private void LoadSettings()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        string currentRes = $"{Screen.currentResolution.width} x {Screen.currentResolution.height}";

        foreach (var res in resolutions)
        {
            string option = $"{res.width} x {res.height}";
            if (!options.Contains(option))
                options.Add(option);
        }

        resolutionDropdown.AddOptions(options);

        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", options.IndexOf(currentRes));
        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);

        OnMasterVolumeChanged(masterSlider.value);
        OnMusicVolumeChanged(musicSlider.value);
        OnSFXVolumeChanged(sfxSlider.value);
    }


    public void OpenSettings() => settingsPanel.SetActive(true);
    public void CloseSettings() => settingsPanel.SetActive(false);

    public void ApplySettings()
    {
        string[] dims = resolutionDropdown.options[resolutionDropdown.value].text.Split('x');
        int width = int.Parse(dims[0].Trim());
        int height = int.Parse(dims[1].Trim());

        Screen.SetResolution(width, height, fullscreenToggle.isOn);

        SetMixerVolume("MasterVolume", masterSlider.value);
        SetMixerVolume("MusicVolume", musicSlider.value);
        SetMixerVolume("SFXVolume", sfxSlider.value);

        SaveSettings();
        Debug.Log("Settings applied.");
    }

    private void SaveSettings()
    {
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("Fullscreen", fullscreenToggle.isOn ? 1 : 0);
        PlayerPrefs.SetFloat("MasterVolume", masterSlider.value);
        PlayerPrefs.SetFloat("MusicVolume", musicSlider.value);
        PlayerPrefs.SetFloat("SFXVolume", sfxSlider.value);

        PlayerPrefs.Save();
    }

    public void ResetToDefaults()
    {
        // Set sliders to default values
        masterSlider.value = defaultMasterVolume;
        musicSlider.value = defaultMusicVolume;
        sfxSlider.value = defaultSFXVolume;

        // Apply volumes to mixer immediately (but do not save)
        OnMasterVolumeChanged(masterSlider.value);
        OnMusicVolumeChanged(musicSlider.value);
        OnSFXVolumeChanged(sfxSlider.value);

        //reset fullscreen and resolution
        fullscreenToggle.isOn = false;

        // Try to find default resolution (e.g. highest or current)
        string defaultRes = $"{Screen.currentResolution.width} x {Screen.currentResolution.height}";
        int defaultResIndex = resolutionDropdown.options.FindIndex(o => o.text == defaultRes);
        resolutionDropdown.value = defaultResIndex >= 0 ? defaultResIndex : 0;
        resolutionDropdown.RefreshShownValue();

        Debug.Log("Settings reset to defaults (not saved yet).");
    }

    public void SkipLevel()
    {
        UIGameStatePanel.GoToNextLevel();
    }

    private void SetMixerVolume(string parameter, float sliderValue)
    {
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.001f)) * 20f;
        audioMixer.SetFloat(parameter, volume);
    }

    public void OnMasterVolumeChanged(float value) => SetMixerVolume("MasterVolume", value);
    public void OnMusicVolumeChanged(float value) => SetMixerVolume("MusicVolume", value);
    public void OnSFXVolumeChanged(float value) => SetMixerVolume("SFXVolume", value);


}
