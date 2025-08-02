using System;
using GMTK;
using UnityEngine;

namespace UI
{
	public class UIGameStatePanel : MonoBehaviour
	{
		private GameState _visibleState;
		private bool _visibleIsPlayingState;
		private RunnerControlState _visibleRunnerState;
		
		public GameObject TimeUpGameOverObject;
		public GameObject SuccessGameOverObject;

		public GameObject PlaybackObject;
		public GameObject NotStartedObject;

		void Start()
		{
			//UpdatePanel(RunnerControlState.Playback, false);
		}
		private void OnEnable()
		{
			GameManager.OnGameStateChange += OnStateChange;
			TimelineRunner.OnPlaybackChange +=  OnPlaybackChange;
			TimelineRunner.OnStateChange += OnTRStateChange;

		}

		
		private void OnDisable()
		{
			GameManager.OnGameStateChange -= OnStateChange;
			TimelineRunner.OnPlaybackChange -= OnPlaybackChange;
			TimelineRunner.OnStateChange -= OnTRStateChange;
		}
		
		private void OnStateChange(GameState state)
		{
			UpdatePanel(state, _visibleRunnerState, _visibleIsPlayingState);
		}

		private void OnPlaybackChange(bool playing)
		{
			UpdatePanel(_visibleState,_visibleRunnerState, playing);
		}

		private void OnTRStateChange(RunnerControlState rs)
		{
			UpdatePanel(_visibleState, rs, _visibleIsPlayingState);
		}


		private void UpdatePanel(GameState state, RunnerControlState runnerState, bool playing)
		{
			_visibleState = state;
			_visibleIsPlayingState = playing;
			_visibleRunnerState = runnerState;
			//
			//have we started yet?
			TimeUpGameOverObject.SetActive(state == GameState.TimeIsUp);
			SuccessGameOverObject.SetActive(state == GameState.AllCollected);
			NotStartedObject.SetActive(state == GameState.NotStarted);
			
			//set playback
			if (runnerState == RunnerControlState.Playback && playing)
			{
				PlaybackObject.SetActive(true);
			}
			else
			{
				PlaybackObject.SetActive(false);
			}
			
			// if(state == RunnerControlState.Playback && )
		}
	}
}