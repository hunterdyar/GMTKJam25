using UnityEngine;
using UnityEngine.Audio;

public class VolumeLoader : MonoBehaviour
{
    [SerializeField] private AudioMixer audioMixer;

    void Start()
    {
        float master = PlayerPrefs.GetFloat("MasterVolume", 0.5f);
        float music = PlayerPrefs.GetFloat("MusicVolume", 0.5f);
        float sfx = PlayerPrefs.GetFloat("SFXVolume", 0.5f);

        ApplyVolume("MasterVolume", master);
        ApplyVolume("MusicVolume", music);
        ApplyVolume("SFXVolume", sfx);
    }

    private void ApplyVolume(string parameter, float value)
    {
        float db = Mathf.Log10(Mathf.Max(value, 0.001f)) * 20f;
        audioMixer.SetFloat(parameter, db);
    }
}
