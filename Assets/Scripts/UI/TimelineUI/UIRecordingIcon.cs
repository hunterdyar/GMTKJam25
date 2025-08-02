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
		public Color _primedColor;

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
			if (state == RunnerControlState.WaitingToRecord)
			{
				_image.enabled = true;
				_image.color= _primedColor;
			}else if (state == RunnerControlState.Recording)
			{
				_image.enabled = true;
				_image.color = _recordingColor;
			}
			else
			{
				_image.enabled = false;
				_image.color = Color.clear;
			}
		}
	}
}