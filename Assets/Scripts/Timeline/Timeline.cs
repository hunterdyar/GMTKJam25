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
			var lkg = _checkpoints.Where(x => x.Frame <= frame).OrderByDescending(x => x.Frame).First();
			lkg.RestoreCheckpoint();
			for (long i = lkg.Frame; i < frame; i++)
			{
				if (i % 10 == 0)
				{
					CreateCheckpointAtCurrent();
				}
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
			if (_playbackFrame >= MaxFrame)
			{
				return;
			}
			//add new inputs! todo: playback vs. recording :p
			
			if (_inputsMap.TryGetValue(_playbackFrame, out GameInput input))
			{
				input = MergeInputs(_playbackFrame, input,  _pending);
				_inputsMap[_playbackFrame] = input;
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

		//Merge B into A. Set any releases to playbackFrame. We should be able to discard b after this.
		//todo: arrows... (uhg copy/paste this mess)
		private GameInput MergeInputs(long playbackFrame, GameInput a, GameInput b)
		{
			//take 2: overwrite
			if (a.JumpButton == null)
			{
				a.JumpButton = b.JumpButton;
				b.JumpButton = null;
				return a;
			}

			if (b.JumpButton == null || !b.JumpButton.IsPressed(playbackFrame))
			{
				if (a.JumpButton.ReleaseFrame >= playbackFrame)
				{
					if (a.JumpButton.PressFrame < playbackFrame)
					{
						var whenrelease = playbackFrame - 1;
						if (whenrelease > a.JumpButton.PressFrame)
						{
							a.JumpButton.ReleaseFrame = whenrelease;
						}
						else
						{
							a.JumpButton.ReleaseFrame = a.JumpButton.PressFrame;//invalidate jic.
							a.JumpButton = null;
						}
					}else if(a.JumpButton.PressFrame == playbackFrame)
					{
						var whenpress = playbackFrame + 1;
						if (whenpress < a.JumpButton.ReleaseFrame)
						{
							a.JumpButton.PressFrame = whenpress;
						}
						else
						{
							//delete
							a.JumpButton.PressFrame = whenpress;//invalidate jic. the 'isPressed' function will return false in this case.
							a.JumpButton = null;
						}
						return a;
					}
				}
			}
			else
			{
				//if presing b...
			}
			
			
			//were not pressing, now we are. (and if b is null, it's still fine, still not pressing the button.) also bounds check a, as - if we truncated a previous input, then it may not be valid for this frame anymore so ignore.
			if (a.JumpButton == null || (a.JumpButton.ReleaseFrame < playbackFrame && a.JumpButton.ReleaseFrame != -1) || a.JumpButton.PressFrame == -1 || a.JumpButton.PressFrame > playbackFrame )
			{
				a.JumpButton = b.JumpButton;
				b.JumpButton = null;
			}
			else
			{
				//there is a press with a, and we are causing a release.
				if (b.JumpButton == null || b.JumpButton.PressFrame <= playbackFrame && (b.JumpButton.ReleaseFrame == -1 || b.JumpButton.ReleaseFrame > playbackFrame) && (a.JumpButton.ReleaseFrame != -1 && a.JumpButton.ReleaseFrame >= _playbackFrame))
				{
					//update the "previous" press to release.
					if (a.JumpButton.PressFrame < playbackFrame - 1)
					{
						a.JumpButton.ReleaseFrame = playbackFrame - 1;
					}
					else
					{
						//we perfectly caused an overlap, destroy a, replace with b.
						a.JumpButton = null;
					}

					//create a new press. or non-press, or whatever.
					a.JumpButton = b.JumpButton;
					b.JumpButton = null;
				}
				else
				{
					if (a.JumpButton.PressFrame > b.JumpButton.ReleaseFrame && b.JumpButton.ReleaseFrame != -1)
					{
						//exit... these don't overlap?
					}else if (b.JumpButton.ReleaseFrame != -1 && b.JumpButton.ReleaseFrame < playbackFrame)
					{
						//exit, these... don't overlap?
					}else if (a.JumpButton.Button == b.JumpButton.Button)
					{
						//do nothing, more or less. Catch edge cases for press+press back to back.
						if (b.JumpButton.PressFrame < a.JumpButton.PressFrame)
						{
							a.JumpButton.PressFrame = b.JumpButton.PressFrame;
						}

						if (b.JumpButton.ReleaseFrame == -1 || b.JumpButton.ReleaseFrame > a.JumpButton.ReleaseFrame)
						{
							a.JumpButton.ReleaseFrame = b.JumpButton.ReleaseFrame;
						}

						b.JumpButton = null;
					}
					else
					{
						//we press a button and a is not pressed, or vise-versa.
						if (b.JumpButton.Button == Buttons.Jump)
						{
							Debug.Assert(a.JumpButton.Button == Buttons.None);
							a.JumpButton = b.JumpButton;
							b.JumpButton = null;
						} else if (b.JumpButton.Button == Buttons.None)
						{
							Debug.Assert(a.JumpButton.Button == Buttons.Jump);
							a.JumpButton.ReleaseFrame = _playbackFrame-1;
							a.JumpButton = b.JumpButton;
							b.JumpButton = null;
						}
					}
				}
				
			}

			if (a.ArrowButton == null)
			{
				a.ArrowButton = b.ArrowButton;
				b.ArrowButton = null;
			}
			
			

			return a;
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
		}
	}
}