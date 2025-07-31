using System;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK
{
	public enum RunnerControlState
	{
		Playback,
		Recording,
	}
	public class TimelineRunner : MonoBehaviour
	{
		public static Action<RunnerControlState> OnStateChange;
		public static Action<bool> OnPlaybackChange;
		
		private UITimelineManager _uiTimeline;
		public Timeline Timeline;
		/// <summary>
		/// If we are trying to watch or record the game with a real world clock ticking things.
		/// </summary>
		public bool Playing = false;
		public long testFrame;

		public InputAction _debugCreateCheckpoint;
		public InputAction _debugGoToTestFrame; 
		public long PendingFrame => Timeline.CurrentDisplayedFrame + 1;
		public RunnerControlState State => _state;
		[SerializeField]
		private RunnerControlState _state = RunnerControlState.Playback;
		private void Awake()
		{
			_debugCreateCheckpoint.Enable();
			_debugGoToTestFrame.Enable();
			_state = RunnerControlState.Playback;
			//this is temp (FAMOUSLASTWORDS)
			_uiTimeline = GameObject.FindFirstObjectByType<UITimelineManager>();
		}

		private void Start()
		{
			Playing = false;
			Timeline.Init();
			//save first position.
			Timeline.CreateCheckpointAtCurrent();
			OnStateChange?.Invoke(_state);
			OnPlaybackChange?.Invoke(Playing);
		}

		private void Update()
		{
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

		public void PlayPauseToggle()
		{
			Playing = !Playing;
			OnPlaybackChange?.Invoke(Playing);
		}
		public void ToggleRecordState()
		{
			if (_state == RunnerControlState.Playback)
			{
				//todo: fine for now, enter playback when not recording.
				if (!Playing)
				{
					PlayPauseToggle();
				}
				_state = RunnerControlState.Recording;
			}else if (_state == RunnerControlState.Recording)
			{
				Timeline.ForceReleaseInput(Timeline.CurrentDisplayedFrame);
				//i know we don't actually need the else but I am anticipating future states.
				_state = RunnerControlState.Playback;

				//pause when exiting record mode....

				//todo we should only do this if we have "caught up" to the last frame?
				PauseIfPlaying();
			}

			OnStateChange?.Invoke(_state);
		}

		public void PauseIfPlaying()
		{
			if (Playing)
			{
				PlayPauseToggle();
			}
		}
	}
}