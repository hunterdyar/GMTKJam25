using System;
using System.Linq;
using GMTK;
using UnityEngine;

public enum GameState
{
	NotStarted,
	Playing,
	TimeIsUp,
	AllCollected,
}
namespace GMTK
{
	public class GameManager : TimelineListener
	{
		public static Action<GameState> OnGameStateChange;
		public long Score => _timeline.CurrentDisplayedFrame;

		public GameState GameState => _gameState;
		//only serialized for testing to see it in the inspector.
		[SerializeField] private GameState _gameState;
		
		private Collectable[] _collectables;

		void Awake()
		{
			_collectables = GameObject.FindObjectsByType<Collectable>(FindObjectsSortMode.None);
			_gameState = GameState.NotStarted;
		}

		void Start()
		{
			OnGameStateChange?.Invoke(_gameState);
		}
		
		private void OnInput(long frame, GameInput input, bool instant)
		{
			if (frame <= 1)
			{
				SetGameState(GameState.NotStarted);
			}else if (frame >= _timeline.MaxFrame)
			{
				SetGameState(GameState.TimeIsUp);
			}else if (_collectables.All(x => x.HasBeenInteractedWith))
			{
				SetGameState(GameState.AllCollected);
			}
			else
			{
				SetGameState(GameState.Playing);
			}
		}

		public void SetGameState(GameState newState)
		{
			if (_gameState != newState)
			{
				_gameState = newState;
				OnGameStateChange?.Invoke(newState);
			}
		}
		
		
		private void OnEnable()
		{
			Timeline.OnInput += OnInput;
		}

		private void OnDisable()
		{
			Timeline.OnInput -= OnInput;
		}

		protected override CheckpointData GetCheckpointData()
		{
			return new GameManagerCheckpointData(this)
			{
				_state = _gameState
			};
		}
		
	}
}