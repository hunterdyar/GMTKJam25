using System;
using GMTK;
using UnityEngine;

namespace UI
{
	public class UIGameStatePanel : MonoBehaviour
	{
		private GameState _visibleState;
		private bool _visibleIsPlayingState;
		
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
		}

		private void OnDisable()
		{
			GameManager.OnGameStateChange -= OnStateChange;
		}
		
		private void OnStateChange(GameState state)
		{
			UpdatePanel(state, _visibleIsPlayingState);
		}

		private void OnPlaybackChange(bool playing)
		{
			UpdatePanel(_visibleState, playing);
		}

		private void UpdatePanel(GameState state, bool playing)
		{
			_visibleState = state;
			_visibleIsPlayingState = playing;
			//
			//have we started yet?
			TimeUpGameOverObject.SetActive(state == GameState.TimeIsUp);
			SuccessGameOverObject.SetActive(state == GameState.AllCollected);
			NotStartedObject.SetActive(state == GameState.NotStarted);
			
			//set playback
			if (state == GameState.Playing && playing)
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