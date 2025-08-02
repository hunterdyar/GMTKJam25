using System;
using UI;
using UnityEngine;
using UnityEngine.InputSystem;

namespace GMTK
{
	public enum RunnerControlState
	{
		Playback,
		Scrubbing,
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
		public int testFrame;

		public InputAction _debugCreateCheckpoint;
		public InputAction _debugGoToTestFrame; 
		public int PendingFrame => Timeline.CurrentDisplayedFrame + 1;
		public RunnerControlState State => _state;
		[SerializeField]
		private RunnerControlState _state = RunnerControlState.Playback;

		private GameManager _gameManager;
		private void Awake()
		{
			_gameManager = GetComponent<GameManager>();
			_debugCreateCheckpoint.Enable();
			_debugGoToTestFrame.Enable();
			_state = RunnerControlState.Playback;
			//this is temp (FAMOUSLASTWORDS)
			_uiTimeline = GameObject.FindFirstObjectByType<UITimelineManager>();
		}

		void OnEnable()
		{
			GameManager.OnGameStateChange += OnGameStateChange;
		}

		void OnDisable()
		{
			GameManager.OnGameStateChange -= OnGameStateChange;
		}
		private void OnGameStateChange(GameState gameState)
		{
			if (gameState == GameState.AllCollected || gameState == GameState.TimeIsUp)
			{
				PauseIfPlaying(true);
				if (_state == RunnerControlState.Recording)
				{
					ToggleRecordState(true);
					Timeline.SetDirtyAfter(Timeline.CurrentDisplayedFrame);
				}
			}
		}

		private void Start()
		{
			_state = RunnerControlState.Playback;
			Playing = false;
			Timeline.Init(30*50);
			//save first position.
			OnStateChange?.Invoke(_state);
			OnPlaybackChange?.Invoke(Playing);
		}

		private void Update()
		{
			if (_debugCreateCheckpoint.WasPerformedThisFrame())
			{
				// Timeline.CreateCheckpointAtCurrent();
			}

			if (_debugGoToTestFrame.WasPerformedThisFrame())
			{
				Timeline.GoToFrame(testFrame);
			}
		}

		private void FixedUpdate()
		{
			if (Playing && _gameManager.GameState == GameState.PlayingOrRecording ||_gameManager.GameState == GameState.NotStarted)
			{
				Timeline.Tick(_state == RunnerControlState.Recording);
			}
		}

		/// <param name="force">Ignore Current game state condition</param>
		public void PlayPauseToggle(bool force = false)
		{
			if (force || (_gameManager.GameState != GameState.TimeIsUp && _gameManager.GameState != GameState.AllCollected))
			{
				Playing = !Playing;
				OnPlaybackChange?.Invoke(Playing);
			}
		}
		public void ToggleRecordState(bool force = false)
		{
			if ((_gameManager.GameState == GameState.TimeIsUp || _gameManager.GameState == GameState.AllCollected))
			{
				if (!force)
				{
					return;
				}
			}

			if (_state == RunnerControlState.Playback)
			{
				//todo: fine for now, enter playback when not recording.
				if (!Playing)
				{
					PlayPauseToggle(force);
				}
				_state = RunnerControlState.Recording;
				OnStateChange?.Invoke(_state);
				return;
			}else if (_state == RunnerControlState.Recording)
			{
				Timeline.ForceReleaseInput(Timeline.CurrentDisplayedFrame);
				//i know we don't actually need the else but I am anticipating future states.
				_state = RunnerControlState.Playback;

				//pause when exiting record mode....

				//todo we should only do this if we have "caught up" to the last frame?
				PauseIfPlaying();
				OnStateChange?.Invoke(_state);
				return;
			}
		}

		public void ScrubJumpToFrame(int frame)
		{
			StopRecordingIfRecording();
			Timeline.GoToFrame(frame);
		}

		public void StepForwardOne()
		{
			StopRecordingIfRecording();
			Timeline.StepForwardOneFrame();
		}

		public void PauseIfPlaying(bool force = false)
		{
			if (Playing || force)
			{
				PlayPauseToggle(force);
			}
		}
		
		private void StopRecordingIfRecording()
		{
			if (_state == RunnerControlState.Recording)
			{
				Timeline.ForceReleaseInput(Timeline.CurrentDisplayedFrame);
				//i know we don't actually need the else but I am anticipating future states.
				_state = RunnerControlState.Playback;
				OnStateChange?.Invoke(_state);
			}
		}

		public void SetIsScrubbing(bool isScrubbing)
		{
			
			if (isScrubbing)
			{
				if (_state == RunnerControlState.Recording)
				{
					PauseIfPlaying();
				}
				
				if (_state != RunnerControlState.Scrubbing)
				{
					_state = RunnerControlState.Scrubbing;
					OnStateChange?.Invoke(_state);
				}
			}
			else
			{
				if (_state == RunnerControlState.Scrubbing)
				{
					_state = RunnerControlState.Playback;
					OnStateChange?.Invoke(_state);
				}
			}
		}
	}
}