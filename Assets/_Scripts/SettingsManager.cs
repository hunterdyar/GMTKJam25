using UnityEngine;
using UnityEngine.UI;
using TMPro;
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

    [SerializeField] private float defaultMasterVolume = 0.5f;
    [SerializeField] private float defaultMusicVolume = 0.5f;
    [SerializeField] private float defaultSFXVolume = 0.5f;

    private Resolution[] resolutions;

    private void Start()
    {
        // Populate resolutions
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();

        var options = new System.Collections.Generic.List<string>();
        string currentRes = $"{Screen.currentResolution.width} x {Screen.currentResolution.height}";

        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = $"{resolutions[i].width} x {resolutions[i].height}";
            if (!options.Contains(option))
                options.Add(option);
        }

        resolutionDropdown.AddOptions(options);

        // Load saved settings or use defaults
        int savedResIndex = PlayerPrefs.GetInt("ResolutionIndex", options.IndexOf(currentRes));
        resolutionDropdown.value = savedResIndex;
        resolutionDropdown.RefreshShownValue();

        fullscreenToggle.isOn = PlayerPrefs.GetInt("Fullscreen", Screen.fullScreen ? 1 : 0) == 1;

        masterSlider.value = PlayerPrefs.GetFloat("MasterVolume", defaultMasterVolume);
        musicSlider.value = PlayerPrefs.GetFloat("MusicVolume", defaultMusicVolume);
        sfxSlider.value = PlayerPrefs.GetFloat("SFXVolume", defaultSFXVolume);

        ApplySettings(); // Apply loaded values to game
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

    private void SetMixerVolume(string parameter, float sliderValue)
    {
        float volume = Mathf.Log10(Mathf.Max(sliderValue, 0.001f)) * 20f;
        audioMixer.SetFloat(parameter, volume);
    }

    public void OnMusicVolumeChanged(float value) => SetMixerVolume("MusicVolume", value);

}
