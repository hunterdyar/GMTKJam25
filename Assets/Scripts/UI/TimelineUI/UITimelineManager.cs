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
		public long StartDisplayFrame => _startDisplayFrame;
		private long _startDisplayFrame;
		public long EndDisplayFrame => _endDisplayFrame;
		private long _endDisplayFrame;

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
		private long _currentViewedDisplayFrame;
		private RectTransform _rectTransform;
		private long HalfFrameCount;
	
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
			_startDisplayFrame = -HalfFrameCount;
			_endDisplayFrame = HalfFrameCount;
		}

		void Start()
		{
			Chips = new List<UIFrameChip>();
			for (int i = 0; i < TimelineLength; i++)
			{
				Chips.Add(CreateFrameChip(i));
			}
			
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
			Timeline.OnCurrentDisplayFrameChanged -= OnCurrentDisplayFrameChanged;
			TimelineRunner.OnStateChange -= OnIsRecordingChange;
		}

		private void OnCurrentDisplayFrameChanged(long frame)
		{
			//this frame should be centered.
			if (_currentViewedDisplayFrame != frame)
			{
				UpdateVisuals();
			}
		}
		private void OnInput(long frame, GameInput input)
		{
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
			//change color of timeline chip.
			
			// if (frame >= StartDisplayFrame && frame <= EndDisplayFrame)
			// {
			// 	int index = (int)(frame - StartDisplayFrame);
			// 	//uh?
			// 	if (index >= 0 && index < Chips.Count)
			// 	{
			// 		Chips[index].UpdateVisuals();
			// 	}
			// }

			//update button things.
			UpdateVisuals();
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
					OnFrameEnterView(nextStart + i, delta);
					OnFrameExitView(_endDisplayFrame - i, delta);
				}
			}
			
			_startDisplayFrame = nextStart;
			_endDisplayFrame = nextEnd;
			_currentViewedDisplayFrame = _timeline.CurrentDisplayedFrame;
		}

		public bool TryGetFrameChip(long frame, out UIFrameChip chip)
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

		public void OnFrameEnterView(long frame, long delta)
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
			}
		}

		public void OnFrameExitView(long frame, long delta)
		{
			//Debug.Log($"Exit View: {frame}");
			int index = (int)(frame - StartDisplayFrame);
			if (index >= 0 && index < Chips.Count)
			{
				Chips[index].UpdateVisuals();
			}

			if (_timeline.TryGetFrame(frame, out var input))
			{
				if (input.JumpButton != null)
				{
					if((frame == input.JumpButton.ReleaseFrame && delta < 0)
					   || (frame == input.JumpButton.PressFrame && delta > 0))
					{
						if (VisibleButtonEvents.Contains(input.JumpButton))
						{
							RemoveVisibleButton(input.JumpButton);
						}
					}
				}

				if (input.ArrowButton != null)
				{
					if ((frame == input.ArrowButton.ReleaseFrame && delta < 0)
					    || (frame == input.ArrowButton.PressFrame && delta > 0))
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
				var chip = GetButtonChip(buttonEvent);
				chip.Init(this, buttonEvent);
				_buttonChips.Add(buttonEvent,chip);
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
				_buttonChips[buttonEvent].gameObject.SetActive(false);
				_buttonChips.Remove(buttonEvent);
			}
			// else
			// {
			// 	//uh, okay!
			// }
		}

		private UIButtonChip GetButtonChip(ButtonEvent be)
		{
			if (_buttonChips.TryGetValue(be, out var chip))
			{
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

		public (float,float) GetFramePosition(ButtonEvent be)
		{

			var s = be.PressFrame;
			var e = be.ReleaseFrame;
			if (s < _startDisplayFrame)
			{
				s = _startDisplayFrame;
			}

			if (e > _endDisplayFrame)
			{
				e = _endDisplayFrame;
			}

			var b = 0f;
			if (e == -1)
			{
				b = _rightEdgePoint;
			}
			else
			{
				var et = Mathf.InverseLerp(StartDisplayFrame, EndDisplayFrame, e);
				b = _rectTransform.rect.width * et;

			}

			var st = Mathf.InverseLerp(StartDisplayFrame, EndDisplayFrame, s);
			var a = _rectTransform.rect.width * st;
			return (a, b);
		}
	}
}