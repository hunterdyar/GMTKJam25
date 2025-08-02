using System;
using GMTK;
using UnityEngine;
using UnityEngine.Rendering;

public class VolumeScrubChanger : MonoBehaviour
{
    private Volume _volume;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Awake()
    {
        _volume = GetComponent<Volume>();
        _volume.weight = 0f;
    }


    private void OnEnable()
    {
        TimelineRunner.OnStateChange += OnStateChange;
    }

    private void OnDisable()
    {
        TimelineRunner.OnStateChange -= OnStateChange;

    }
    private void OnStateChange(RunnerControlState state)
    {
        if (state == RunnerControlState.Scrubbing)
        {
            _volume.weight = 1f;
        }
        else
        {
            _volume.weight = 0f;
        }
    }
    
}
