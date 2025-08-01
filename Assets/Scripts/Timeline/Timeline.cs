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
		public static Action<long, GameInput, bool> OnInput;
		public long CurrentDisplayedFrame => _playbackFrame;
		private long _playbackFrame;
		
		private readonly Dictionary<long, GameInput> _inputsMap = new Dictionary<long, GameInput>();

		private readonly List<Checkpoint> _checkpoints = new List<Checkpoint>();
		private readonly List<TimelineListener> _listeners = new List<TimelineListener>();

		private GameInput _pending;
		private long _lastFrame;
		private long _dirtyFrame;
		public long MaxFrame;

		public void Init(long max)
		{
			MaxFrame = max;
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

			if (frame > MaxFrame)
			{
				frame = MaxFrame;
			}

			if (frame == _playbackFrame)
			{
				//
				return;
			}
			
			//restore to next available checkpoint.
			var lkg = _checkpoints.Where(x => x.Frame < frame && x.Frame < _dirtyFrame).OrderByDescending(x => x.Frame).First();
			lkg.RestoreCheckpoint();
			if (lkg.Frame == frame)
			{
				//the <= in the Where is an = so we always do a TickFrame
				//this is so we update visuals. it's a hack! could be better!
				//return;
			}
			for (long i = lkg.Frame; i < frame; i++)
			{
				if (i % 10 == 0)
				{
					CreateCheckpointAtCurrent();
				}
				_playbackFrame = i;
				TickFrame(i, true);
			}
			
			//now we are caught up to this frame, let's play this frame back out.
			_playbackFrame = frame;
			TickFrame(frame, false);
			OnCurrentDisplayFrameChanged?.Invoke(frame);
			CreateCheckpointAtCurrent();
		}

		public void SetDirtyAfter(long frame)
		{
			_dirtyFrame = frame+1;
		}

		public void ClearDirtyCheckpoints()
		{
			for (int i = _checkpoints.Count - 1; i >= 0; i--)
			{
				if (_checkpoints[i].Frame >= _dirtyFrame)
				{
					_checkpoints.RemoveAt(i);
				}
			}
		}
		
		public void Tick(bool recording = false)
		{
			_playbackFrame++;
			if (_playbackFrame >= MaxFrame)
			{
				return;
			}
			//add new inputs! todo: playback vs. recording :p
			if (recording)
			{
				if (_inputsMap.TryGetValue(_playbackFrame, out GameInput input))
				{
					input = MergeInputs(_playbackFrame, input, _pending);
					_inputsMap[_playbackFrame] = input;
					//if we updated things?
					_dirtyFrame = _playbackFrame;
				}
				else
				{
					if (_pending.Any())
					{
						_inputsMap.Add(_playbackFrame, _pending);
					}
				}
			}

			TickFrame(_playbackFrame, false);
			OnCurrentDisplayFrameChanged?.Invoke(_playbackFrame);
			_pending = GameInput.None;
		}

		//Merge B into A. Set any releases to playbackFrame. We should be able to discard b after this.

		private void MergeButtonEvent(long playbackFrame, ref ButtonEvent a, ref ButtonEvent b)
		{
			if (a == null)
			{
				a = b;
				b = null;
				return;
			}

			if (b == null || !b.IsPressed(playbackFrame))
			{
				if (a.ReleaseFrame >= playbackFrame)
				{
					if (a.PressFrame < playbackFrame)
					{
						var whenrelease = playbackFrame - 1;
						if (whenrelease > a.PressFrame)
						{
							a.ReleaseFrame = whenrelease;
						}
						else
						{
							a.ReleaseFrame = a.PressFrame;//invalidate jic.
							a = null;
						}
					}else if(a.PressFrame == playbackFrame)
					{
						var whenpress = playbackFrame + 1;
						if (whenpress < a.ReleaseFrame)
						{
							a.PressFrame = whenpress;
						}
						else
						{
							//delete
							a.PressFrame = whenpress;//invalidate jic. the 'isPressed' function will return false in this case.
							a = null;
						}
						return;
					}
				}
			}
			else
			{
				//if presing b...
			}
			
			
			//were not pressing, now we are. (and if b is null, it's still fine, still not pressing the button.) also bounds check a, as - if we truncated a previous input, then it may not be valid for this frame anymore so ignore.
			if (a == null || (a.ReleaseFrame < playbackFrame && a.ReleaseFrame != -1) || a.PressFrame == -1 || a.PressFrame > playbackFrame )
			{
				a = b;
				b = null;
			}
			else
			{
				//there is a press with a, and we are causing a release.
				if (b == null || b.PressFrame <= playbackFrame && (b.ReleaseFrame == -1 || b.ReleaseFrame > playbackFrame) && (a.ReleaseFrame != -1 && a.ReleaseFrame >= _playbackFrame))
				{
					//update the "previous" press to release.
					if (a.PressFrame < playbackFrame - 1)
					{
						a.ReleaseFrame = playbackFrame - 1;
					}
					else
					{
						//we perfectly caused an overlap, destroy a, replace with b.
						a = null;
					}

					//create a new press. or non-press, or whatever.
					a = b;
					b = null;
				}
				else
				{
					if (a.PressFrame > b.ReleaseFrame && b.ReleaseFrame != -1)
					{
						//exit... these don't overlap?
					}else if (b.ReleaseFrame != -1 && b.ReleaseFrame < playbackFrame)
					{
						//exit, these... don't overlap?
					}else if (a.Button == b.Button)
					{
						//do nothing, more or less. Catch edge cases for press+press back to back.
						if (b.PressFrame < a.PressFrame)
						{
							a.PressFrame = b.PressFrame;
						}

						if (b.ReleaseFrame == -1 || b.ReleaseFrame > a.ReleaseFrame)
						{
							a.ReleaseFrame = b.ReleaseFrame;
						}

						b = null;
					}
					else
					{
						//we press a button and a is not pressed, or vise-versa.
						if (b.Button == Buttons.Jump)
						{
							Debug.Assert(a.Button == Buttons.None);
							a = b;
							b = null;
						} else if (b.Button == Buttons.None)
						{
							Debug.Assert(a.Button == Buttons.Jump);
							a.ReleaseFrame = _playbackFrame-1;
							a = b;
							b = null;
						}
					}
				}
			}
		}
		
		private GameInput MergeInputs(long playbackFrame, GameInput a, GameInput b)
		{
			MergeButtonEvent(playbackFrame, ref a.JumpButton, ref b.JumpButton);
			MergeButtonEvent(playbackFrame, ref a.ArrowButton, ref b.ArrowButton);
			return a;
		}

		public void SetInputOnNextTickedFrame(GameInput input)
		{
			_pending = input;
		}

		public void TickFrame(long frame, bool instant)
		{
			if (_inputsMap.TryGetValue(frame, out var input))
			{
				BroadcastEvent(_playbackFrame, input, instant);
			}
			else
			{
				BroadcastEvent(_playbackFrame, GameInput.None, instant);
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

		private void BroadcastEvent(long frame, GameInput input, bool instant)
		{
			OnInput?.Invoke(frame, input, instant);
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

		public void ForceReleaseInput(long frame)
		{
			if (_inputsMap.TryGetValue(frame, out var input))
			{
				if (input.JumpButton != null && input.JumpButton.ReleaseFrame == -1)
				{
					input.JumpButton.ReleaseFrame = frame;
				}

				if (input.ArrowButton != null && input.ArrowButton.ReleaseFrame == -1)
				{
					input.ArrowButton.ReleaseFrame = frame;
				}

				if (input.ArrowButtonB != null && input.ArrowButtonB.ReleaseFrame == -1)
				{
					input.ArrowButtonB.ReleaseFrame = frame;
				}
			}

			_pending = GameInput.None;
		}
	}
}