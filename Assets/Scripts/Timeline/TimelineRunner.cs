using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK
{
	public class TimelineRunner : MonoBehaviour
	{
		public Timeline Timeline;
		public bool Playing = false;

		public long testFrame;

		public InputAction _debugCreateCheckpoint;
		public InputAction _debugGoToTestFrame;
		public InputActionReference _playbackToggle;

		private void Awake()
		{
			_debugCreateCheckpoint.Enable();
			_debugGoToTestFrame.Enable();
			_playbackToggle.action.Enable();
		}

		private void Start()
		{
			Timeline.Init();
			//save first position.
			Timeline.CreateCheckpointAtCurrent();
		}

		private void Update()
		{
			if (_playbackToggle.action.WasPerformedThisFrame())
			{
				Playing = !Playing;
			}

			if (_debugCreateCheckpoint.WasPerformedThisFrame())
			{
				Timeline.CreateCheckpointAtCurrent();
			}

			if (_debugGoToTestFrame.WasPerformedThisFrame())
			{
				Timeline.GoToFrame(testFrame);
			}
		}

		private void FixedUpdate()
		{
			if (Playing)
			{
				Timeline.Tick();
			}
		}
	}
}