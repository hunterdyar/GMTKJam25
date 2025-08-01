using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GMTK
{
	[CreateAssetMenu(fileName = "Timeline", menuName = "Jam/Timeline", order = 0)]
	public class Timeline : ScriptableObject
	{
		public static Action<int> OnCurrentDisplayFrameChanged;
		public static Action<int, GameInput, bool> OnInput;
		public int CurrentDisplayedFrame => _playbackFrame;
		private int _playbackFrame;
		
		private GameInput[] _inputHistory;
		private Checkpoint[] _checkpointHistory;
		private readonly List<TimelineListener> _listeners = new List<TimelineListener>();

		private GameInput _pending;
		private int _lastFrame;
		private int _dirtyFrame;
		public int MaxFrame;
		private int _lastTickedFrame;
		public bool alwaysCreateCheckpoints;
		public void Init(int max)
		{
			_lastTickedFrame = -2;
			MaxFrame = max;
			_playbackFrame = 0;
			_lastFrame = 0;
			_inputHistory = new GameInput[MaxFrame+1];
			_checkpointHistory = new Checkpoint[MaxFrame + 1];
			// _listeners.Clear();//they should all deregister, and if they don't i want to know about it.
			Physics.simulationMode = SimulationMode.Script;
			CreateInitialCheckpoint();
		}

		public void GoToFrame(int frame)
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
			
			//restore to next available (clean) checkpoint.
			Checkpoint lkg = _checkpointHistory[0];
			for (int i = Mathf.Min(_dirtyFrame,frame); i >= 0; i--)
			{
				if ( _checkpointHistory[i] != null)
				{
					lkg = _checkpointHistory[i];
					break;
				}
			}
			lkg.RestoreCheckpoint();
			_playbackFrame = lkg.Frame;
			_lastTickedFrame = lkg.Frame;//cheating this variable...
			
			if (lkg.Frame == frame)
			{
				//the <= in the Where is an = so we always do a TickFrame
				//this is so we update visuals. it's a hack! could be better!
				//return;
			}
			else
			{
				for (int i = lkg.Frame + 1; i <= frame; i++)
				{
					_playbackFrame = i;
					TickFrame(i, i == frame, false);
				}
			}

			OnCurrentDisplayFrameChanged?.Invoke(frame);
			//CreateCheckpointAtCurrent();
		}

		public void StepForwardOneFrame()
		{
			_playbackFrame = _lastTickedFrame + 1;
			TickFrame(_playbackFrame, false);
		}

		public void SetDirtyAfter(int frame)
		{
			_dirtyFrame = frame+1;
		}

		public void ClearDirtyCheckpoints()
		{
			for (int i = _dirtyFrame; i <= MaxFrame; i++)
			{
				_checkpointHistory[i] = null;
			}
		}
		
		public void Tick(bool recording = false)
		{
			_playbackFrame++;
			if (_playbackFrame >= MaxFrame || _playbackFrame < 0)
			{
				return;
			}
			//add new inputs! todo: playback vs. recording :p
			if (recording)
			{
				var input = _inputHistory[_playbackFrame];
				
				//these were -1, then we updated or changed history. so they are invalid and can be discarded. 
				//all the checks for this are uh, still around in rest of the code, but moving it here cleans a lot of that up, since we won't broadcast an out-of-range input button.
				if (input.JumpButton != null && input.JumpButton.ReleaseFrame != -1 && (input.JumpButton.ReleaseFrame < _playbackFrame || input.JumpButton.PressFrame > _playbackFrame))
				{
					input.JumpButton = null;
				}

				if (input.ArrowButton != null && input.ArrowButton.ReleaseFrame != -1 && (input.ArrowButton.ReleaseFrame < _playbackFrame || input.ArrowButton.PressFrame > _playbackFrame))
				{
					input.ArrowButton= null;
				}
				
				input = MergeInputs(_playbackFrame, input, _pending);
				_inputHistory[_playbackFrame] = input;
				//if we updated things?
				_dirtyFrame = _playbackFrame;
			}

			TickFrame(_playbackFrame, false, _playbackFrame == 0 || _playbackFrame % 64 == 0);
			OnCurrentDisplayFrameChanged?.Invoke(_playbackFrame);
			_pending = GameInput.None;
		}

		//Merge B into A. Set any releases to playbackFrame. We should be able to discard b after this.

		private void MergeButtonEvent(int playbackFrame, ref ButtonEvent a, ref ButtonEvent b)
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
		
		private GameInput MergeInputs(int playbackFrame, GameInput a, GameInput b)
		{
			MergeButtonEvent(playbackFrame, ref a.JumpButton, ref b.JumpButton);
			MergeButtonEvent(playbackFrame, ref a.ArrowButton, ref b.ArrowButton);
			return a;
		}

		public void SetInputOnNextTickedFrame(GameInput input)
		{
			_pending = input;
		}

		public void TickFrame(int frame, bool instant, bool createCheckpoint = false)
		{
			Debug.Log("tick "+frame);
			if (frame == _lastTickedFrame)
			{
				Debug.LogWarning("are you sure about that? ");
				return;
			}

			if (frame > MaxFrame)
			{
				return;
			}

			var input = _inputHistory[frame];
			BroadcastEvent(frame, input, instant);
			Physics.Simulate(Time.fixedDeltaTime);
			_lastFrame = frame > _lastFrame ? frame : _lastFrame;
			_lastTickedFrame = frame;

			if (createCheckpoint || alwaysCreateCheckpoints)
			{
				var c = new Checkpoint(frame, _listeners.Count);
				foreach (var listener in _listeners)
				{
					listener.SaveCurrentSelfToCheckpoint(ref c);
				}

				_checkpointHistory[frame] = c;
			}
		}

		void CreateInitialCheckpoint()
		{
			if (_playbackFrame > 0)
			{
				Debug.LogError("whoops");
			}
			var c = new Checkpoint(0, _listeners.Count);
			foreach (var listener in _listeners)
			{
				listener.SaveCurrentSelfToCheckpoint(ref c);
			}

			_checkpointHistory[0] = c;
		}


		private void BroadcastEvent(int frame, GameInput input, bool instant)
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

		public bool TryGetFrame(int frame, out GameInput input)
		{
			if(frame < 0 || frame >=MaxFrame)
			{
				input = GameInput.None;
				return false;
			}
			input = _inputHistory[frame];
			return true;
		}

		public int LastFrame()
		{
			return _lastFrame;
		}

		public void ForceReleaseInput(int frame)
		{
			if (frame < 0 || frame >= MaxFrame)
			{
				return;
			}
			var input = _inputHistory[frame];
			
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
			// _inputHistory[frame] = input;
			_pending = GameInput.None;
		}
	}
}