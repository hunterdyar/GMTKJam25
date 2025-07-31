using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GMTK
{
	[CreateAssetMenu(fileName = "Timeline", menuName = "Jam/Timeline", order = 0)]
	public class Timeline : ScriptableObject
	{
		public static Action<long> OnCurrentDisplayFrameChanged;
		public static Action<long, GameInput> OnInput;
		public long CurrentDisplayedFrame => _playbackFrame;
		private long _playbackFrame;
		
		private readonly Dictionary<long, GameInput> _inputsMap = new Dictionary<long, GameInput>();

		private readonly List<Checkpoint> _checkpoints = new List<Checkpoint>();
		private readonly List<TimelineListener> _listeners = new List<TimelineListener>();

		private GameInput _pending;
		private long _lastFrame;

		public void Init()
		{
			_playbackFrame = -1;
			_lastFrame = 0;
			_inputsMap.Clear();
			_checkpoints.Clear();
			Physics.simulationMode = SimulationMode.Script;
		}

		public void GoToFrame(long frame)
		{
			//clamp
			if (frame < 0)
			{
				frame = 0;
			}

			//how, really do we clamp here? we CAN jump into the future, just no inputs.
			if (frame > _lastFrame)
			{
				frame = _lastFrame;
			}

			if (frame == _playbackFrame)
			{
				//
				return;
			}
			
			//restore to next available checkpoint.
			var lkg = _checkpoints.Where(x => x.Frame <= frame).OrderByDescending(x => x.Frame).First();
			lkg.RestoreCheckpoint();
			for (long i = lkg.Frame; i < frame; i++)
			{
				_playbackFrame = i;
				TickFrame(i);
			}
			
			//now we are caught up to this frame, let's play this frame back out.
			_playbackFrame = frame;
			TickFrame(frame);
			OnCurrentDisplayFrameChanged?.Invoke(frame);
			CreateCheckpointAtCurrent();
		}

		public void SetDirtyAfter(long frame)
		{
			for (int i = _checkpoints.Count - 1; i >= 0; i--)
			{
				if (_checkpoints[i].Frame > frame)
				{
					_checkpoints.RemoveAt(i);
				}
			}

			_lastFrame = frame;
		}
		
		public void Tick()
		{
			_playbackFrame++;
			//add new inputs! todo: playback vs. recording :p
			
			if (_inputsMap.TryGetValue(_playbackFrame, out GameInput input))
			{
				if (_pending.Any())
				{
					//replace! bad but hey.
					_inputsMap[_playbackFrame] = _pending;
				}
			}
			else
			{
				if (_pending.Any())
				{
					_inputsMap.Add(_playbackFrame, _pending);
				}
			}

			TickFrame(_playbackFrame);
			OnCurrentDisplayFrameChanged?.Invoke(_playbackFrame);
			_pending = GameInput.None;
		}

		public void SetInputOnNextTickedFrame(GameInput input)
		{
			_pending = input;
		}

		public void TickFrame(long frame)
		{
			if (_inputsMap.TryGetValue(frame, out var input))
			{
				BroadcastEvent(_playbackFrame, input);
			}

			Physics.Simulate(Time.fixedUnscaledDeltaTime);
			_lastFrame = frame > _lastFrame ? frame : _lastFrame;
		}

		public void CreateCheckpointAtCurrent()
		{
			var c = new Checkpoint(_playbackFrame);
			foreach (var listener in _listeners)
			{
				listener.SaveCurrentSelfToCheckpoint(ref c);
			}

			_checkpoints.Add(c);
		}

		private void BroadcastEvent(long frame, GameInput input)
		{
			OnInput?.Invoke(frame, input);
		}

		public void AddTimelineListener(TimelineListener timelineListener)
		{
			if (!_listeners.Contains(timelineListener))
			{
				_listeners.Add(timelineListener);
			}
			else
			{
				Debug.LogWarning("Listener already in timeline", timelineListener);
			}
		}

		public void RemoveTimelineListener(TimelineListener timelineListener)
		{
			if (_listeners.Contains(timelineListener))
			{
				_listeners.Remove(timelineListener);
			}
			else
			{
				Debug.LogWarning("Listener not in in timeline", timelineListener);
			}
		}

		public bool TryGetFrame(long frame, out GameInput input)
		{
			return _inputsMap.TryGetValue(frame, out input);
		}

		public long LastFrame()
		{
			return _lastFrame;
		}
	}
}