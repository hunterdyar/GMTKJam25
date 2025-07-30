using System;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK
{
	public class TimelineRunner : MonoBehaviour
	{
		private UITimelineManager _uiTimeline;
		public Timeline Timeline;
		public bool Playing = false;

		public long testFrame;

		public InputAction _debugCreateCheckpoint;
		public InputAction _debugGoToTestFrame;
		public InputActionReference _playbackToggle;
		public long PendingFrame => Timeline._playbackFrame + 1;

		private void Awake()
		{
			_debugCreateCheckpoint.Enable();
			_debugGoToTestFrame.Enable();
			_playbackToggle.action.Enable();
			
			//this is temp (FAMOUSLASTWORDS)
			_uiTimeline = GameObject.FindFirstObjectByType<UITimelineManager>();
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

			if (_uiTimeline != null)
			{
				_uiTimeline.SetStartAndEnd(Timeline._playbackFrame - _uiTimeline.TimelineLength, Timeline._playbackFrame);
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