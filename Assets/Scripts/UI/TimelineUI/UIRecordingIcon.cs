using System;
using GMTK;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	[RequireComponent(typeof(Image))]
	public class UIRecordingIcon : MonoBehaviour
	{
		private Image _image;
		public bool DisableWhenNotRecording;
		public Color _recordingColor;
		public Color _playbackColor;

		private void Awake()
		{
			_image = GetComponent<Image>();
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
			SetStateIcon(state);
		}

		private void SetStateIcon(RunnerControlState state)
		{
			var recording = state == RunnerControlState.Recording;
			if (DisableWhenNotRecording)
			{
				_image.enabled = recording;
			}
			
			
			_image.color = recording ? _recordingColor : _playbackColor;
			
		}
	}
}