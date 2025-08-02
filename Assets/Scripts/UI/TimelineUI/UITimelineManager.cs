using System;
using System.Collections.Generic;
using GMTK;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UITimelineManager : MonoBehaviour
	{
		public int StartDisplayFrame => _startDisplayFrame;
		private int _startDisplayFrame;
		public int EndDisplayFrame => _endDisplayFrame;
		private int _endDisplayFrame;

		public int TimelineLength = 100;
		public List<UIFrameChip> Chips;

		[Header("Scene Config")] public Transform FrameTimelineParent;
		public Transform ButtonChipParent;
		public Timeline Timeline => _timeline;//public getter
		public Action OnPositionUpdate;

		[Header("Config")] [SerializeField] Timeline _timeline;
		public UIFrameChip FrameChipPrefab;
		public UIButtonChip ButtonChipPrefab;

		public List<ButtonEvent> VisibleButtonEvents = new List<ButtonEvent>();
		public List<UIButtonChip> ButtonChips = new List<UIButtonChip>();
		private Dictionary<ButtonEvent, UIButtonChip> _buttonChips = new Dictionary<ButtonEvent, UIButtonChip>();
		private UICurrentFrameChip _currentFrameChip;
		private UIFrameChip _rightEdgeChip;
		private float _rightEdgePoint;
		private int _currentViewedDisplayFrame;
		private RectTransform _rectTransform;
		private int HalfFrameCount;
	
		private bool _isRecording;
		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
			HalfFrameCount = TimelineLength / 2;
			_currentFrameChip = GetComponent<UICurrentFrameChip>();
			if (_currentFrameChip != null)
			{
				_currentFrameChip.Init(this);
			}
			_startDisplayFrame = -HalfFrameCount-TimelineLength;
			_endDisplayFrame = HalfFrameCount-TimelineLength;
			
			//
			Chips = new List<UIFrameChip>();
			for (int i = 0; i < TimelineLength; i++)
			{
				Chips.Add(CreateFrameChip(i));
			}
			
		}

		void Start()
		{
			OnPositionUpdate?.Invoke();
		}

		private void OnEnable()
		{
			Timeline.OnInput += OnInput;
			Timeline.OnCurrentDisplayFrameChanged += OnCurrentDisplayFrameChanged;
			TimelineRunner.OnStateChange += OnIsRecordingChange;
		}

		private void OnDisable()
		{
			Timeline.OnInput -= OnInput;
			Timeline.OnCurrentDisplayFrameChanged -= OnCurrentDisplayFrameChanged;
			TimelineRunner.OnStateChange -= OnIsRecordingChange;
		}

		private void OnCurrentDisplayFrameChanged(int frame)
		{
			//this frame should be centered.
			if (_currentViewedDisplayFrame != frame)
			{
				UpdateVisuals();
			}
		}
		private void OnInput(int frame, GameInput input, bool instant)
		{
			//frames can be created at the current position
			if (input.JumpButton != null)
			{
				if (!VisibleButtonEvents.Contains(input.JumpButton))
				{
					AddVisibleButton(input.JumpButton);
				}
			}
			
			if (input.ArrowButton != null)
			{
				if (!VisibleButtonEvents.Contains(input.ArrowButton))
				{
					AddVisibleButton(input.ArrowButton);
				}
			}
			
			if (!instant) 
			{ 
				UpdateVisuals(); 
			}
		}

		public void UpdateVisuals()
		{ 
			var nextStart = _timeline.CurrentDisplayedFrame - HalfFrameCount;
			// _startDisplayFrame = _timeline.CurrentDisplayedFrame - HalfFrameCount;
			var nextEnd = _timeline.CurrentDisplayedFrame + HalfFrameCount;
			// _endDisplayFrame = _timeline.CurrentDisplayedFrame + HalfFrameCount;

			//does our shift... shift?
			Debug.Assert(nextStart - _startDisplayFrame == nextEnd - _endDisplayFrame);
			var delta = nextStart - _startDisplayFrame;
			if (delta == 0)
			{
				return;
			}
			else if(delta > 0)
			{
				if (delta > TimelineLength)
				{
					delta = TimelineLength;
				}
				for (int i = 0; i < delta; i++)
				{
					//we moved the timeline to the left (up to later frames)
					//
					// OnFrameExitView(nextStart + i, delta);
					// OnFrameEnterView(_endDisplayFrame - i, delta);
					OnFrameExitView(_startDisplayFrame + i, delta);
					OnFrameEnterView(nextEnd - i, delta);
				}
				
			}else if (delta < 0)
			{

				if (delta < -TimelineLength)
				{
					delta = -TimelineLength;
				}

				for (int i = 0; i < -delta; i++)
				{
					//we moved the timeline to the right (down to earlier frames)
					//we are moving the 'window' to the left (drag left)
					OnFrameExitView(_endDisplayFrame - i, delta);
					OnFrameEnterView(nextStart + i, delta);
				}
			}
			
			_startDisplayFrame = nextStart;
			_endDisplayFrame = nextEnd;
			_currentViewedDisplayFrame = _timeline.CurrentDisplayedFrame;

			for (var i = VisibleButtonEvents.Count - 1; i >= 0; i--)
			{
				var vb = VisibleButtonEvents[i];
				if (vb.PressFrame == vb.ReleaseFrame)
				{
					RemoveVisibleButton(vb);
					continue;
				}
				
				if (!_buttonChips.ContainsKey(vb))
				{
					if (vb.PressFrame != vb.ReleaseFrame)
					{
						Debug.LogWarning("edge case? why no button chip?" + vb.ToString());
						var chip = GetButtonChip(vb);
						_buttonChips.Add(vb, chip);
					}
				}
				else
				{
					if (vb.PressFrame <= _endDisplayFrame && vb.ReleaseFrame >= _startDisplayFrame &&
					    vb.PressFrame != vb.ReleaseFrame)
					{
						if (!_buttonChips[vb].gameObject.activeInHierarchy)
						{
							_buttonChips[vb].Init(this, vb);
						}
					}//invalid should invalidate itself in Update(). could refactor and do it here but i aint touching the fragile code more
				}
			}
		}

		public bool TryGetFrameChip(int frame, out UIFrameChip chip)
		{
			int index = (int)(frame - StartDisplayFrame);
			if (index >= 0 && index < Chips.Count)
			{
				chip = Chips[index];
				return true;
			}
			chip = null;
			return false;
		}

		public void OnFrameEnterView(int frame, int delta)
		{
			//Debug.Log($"Entering View: {frame}");
			int index = (int)(frame - StartDisplayFrame);
			if (index >= 0 && index < Chips.Count)
			{
				Chips[index].UpdateVisuals();
			}

			//add any new buttons
			if (_timeline.TryGetFrame(frame, out var input))
			{
				if (input.JumpButton != null)
				{
					if ((frame == input.JumpButton.ReleaseFrame && delta < 0)
					    || (frame == input.JumpButton.PressFrame && delta > 0))
					{
						if (!VisibleButtonEvents.Contains(input.JumpButton))
						{
							AddVisibleButton(input.JumpButton);
						}
					}
				}

				if (input.ArrowButton != null)
				{
					if (!VisibleButtonEvents.Contains(input.ArrowButton))
					{
						AddVisibleButton(input.ArrowButton);
					}
				}
			}
		}

		public void OnFrameExitView(int frame, int delta)
		{
			int index = (int)(frame - StartDisplayFrame);
			if (index >= 0 && index < Chips.Count)
			{
				Chips[index].UpdateVisuals();
			}

			if (_timeline.TryGetFrame(frame, out var input))
			{
				if (input.JumpButton != null)
				{
					if((frame == input.JumpButton.ReleaseFrame && delta > 0)
					   || (frame == input.JumpButton.PressFrame && delta < 0))
					{
						if (VisibleButtonEvents.Contains(input.JumpButton))
						{
							RemoveVisibleButton(input.JumpButton);
						}
					}
				}

				if (input.ArrowButton != null)
				{
					if ((frame+1 == input.ArrowButton.ReleaseFrame && delta > 0)
					    || (frame == input.ArrowButton.PressFrame && delta < 0))
					{
						if (VisibleButtonEvents.Contains(input.ArrowButton))
						{
							RemoveVisibleButton(input.ArrowButton);
						}
					}
				}
			}
		}

		public void AddVisibleButton(ButtonEvent buttonEvent)
		{
			VisibleButtonEvents.Add(buttonEvent);
			if (!_buttonChips.ContainsKey(buttonEvent))
			{
				if (buttonEvent.PressFrame != buttonEvent.ReleaseFrame)
				{
					var chip = GetButtonChip(buttonEvent);
					chip.gameObject.SetActive(true);
					chip.Init(this, buttonEvent);
					_buttonChips.Add(buttonEvent, chip);
				}
				else
				{
					//Debug.Log("Invalid press/release");
				}
			}
			else
			{
				_buttonChips[buttonEvent].gameObject.SetActive(true);
			}
			
		}

		public void RemoveVisibleButton(ButtonEvent buttonEvent)
		{
			VisibleButtonEvents.Remove(buttonEvent);
			// if (_buttonChips.TryGetValue(buttonEvent, out var chip))
			// {
			// 	chip.gameObject.SetActive(false);
			// 	_buttonChips.Remove(buttonEvent);
			// }

			if (_buttonChips.ContainsKey(buttonEvent))
			{
				//Debug.Log($"Removing button ({buttonEvent})", _buttonChips[buttonEvent].gameObject);
				_buttonChips[buttonEvent].gameObject.SetActive(false);
				_buttonChips.Remove(buttonEvent);
			}
			else
			{
				//Debug.LogWarning("we lost one! a chip was added but it's settings changed.");
				var chip = ButtonChips.Find(x=>x.ButtonEvent ==  buttonEvent);
				if (chip != null)
				{
					//Debug.LogWarning("found the lost one!");
					chip.gameObject.SetActive(false);
				}
			}
		}

		private UIButtonChip GetButtonChip(ButtonEvent be)
		{
			
			if (_buttonChips.TryGetValue(be, out var chip))
			{
				chip.gameObject.SetActive(true);
				return chip;
			}
			
			for (int i = 0; i < ButtonChips.Count; i++)
			{
				if (!ButtonChips[i].gameObject.activeSelf)
				{
					ButtonChips[i].gameObject.SetActive(true);
					return ButtonChips[i];
				}
			}
			
			var b = Instantiate(ButtonChipPrefab, ButtonChipParent);
			ButtonChips.Add(b);
			return b;
		}

		private UIFrameChip CreateFrameChip(int index)
		{
			var chip = Instantiate<UIFrameChip>(FrameChipPrefab, FrameTimelineParent);
			chip.SetIndex(index);
			chip.SetManager(this);
			return chip;
		}

		private void OnIsRecordingChange(RunnerControlState state)
		{
			if (state == RunnerControlState.Recording)
			{
				_rightEdgeChip = Chips[(int)HalfFrameCount];
				_rightEdgePoint = (transform as RectTransform).rect.width/2f;
			}
			else
			{
				_rightEdgeChip = Chips[^1];
				_rightEdgePoint = (transform as RectTransform).rect.width;
			}
		}

		public UIFrameChip GetLeftEdgeChip()
		{
			return Chips[0];
		}

		public UIFrameChip GetCurrentEdgeChip()
		{
			return  Chips[(int)HalfFrameCount];
		}

		public UIFrameChip GetRightEdgeChip()
		{
			return _rightEdgeChip;
		}

		public (float,float, bool) GetFramePosition(ButtonEvent be)
		{

			var s = be.PressFrame;
			var e = be.ReleaseFrame;
			bool v = true;
			if (s < _startDisplayFrame)
			{
				if (e < _startDisplayFrame)
				{
					v = false;
				}
				//s = _startDisplayFrame;
			}

			if (e > _endDisplayFrame)
			{
				if (s > _endDisplayFrame)
				{
					v = false;
				}
				//e = _endDisplayFrame;
			}

			var b = 0f;
			if (e == -1)
			{
				b = _rightEdgePoint;
			}
			else
			{
				var et = InverseLerpUnclamped(StartDisplayFrame, EndDisplayFrame, e);
				b = _rectTransform.rect.width * et;

			}

			var st = InverseLerpUnclamped(StartDisplayFrame, EndDisplayFrame, s);
			var a = _rectTransform.rect.width * st;
			return (a, b, v);
		}
		
		private static float InverseLerpUnclamped(float a, float b, float value)
		{
			return (double)a != (double)b
				? (float)(((double)value - (double)a) / ((double)b - (double)a))
				: 0.0f;
		}

		public void InvalidateButton(ButtonEvent buttonEvent, UIButtonChip uiButtonChip)
		{
			if (VisibleButtonEvents.Contains(buttonEvent))
			{
				RemoveVisibleButton(buttonEvent);
			}
			uiButtonChip.gameObject.SetActive(false);
		}
	}
}