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

		private UICurrentFrameChip _currentFrameChip;
		private long _currentViewedDisplayFrame;

		private long HalfFrameCount;
		private void Awake()
		{
			HalfFrameCount = TimelineLength / 2;
			_currentFrameChip = GetComponent<UICurrentFrameChip>();
			if (_currentFrameChip != null)
			{
				_currentFrameChip.Init(this);
			}
		}

		void Start()
		{
			Chips = new List<UIFrameChip>();
			for (int i = 0; i < TimelineLength; i++)
			{
				Chips.Add(CreateChip(i));
			}
			
			OnPositionUpdate?.Invoke();
		}

		private void OnEnable()
		{
			Timeline.OnInput += OnInput;
			Timeline.OnCurrentDisplayFrameChanged += OnCurrentDisplayFrameChanged;
		}

		private void OnDisable()
		{
			Timeline.OnCurrentDisplayFrameChanged -= OnCurrentDisplayFrameChanged;
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
			_startDisplayFrame = _timeline.CurrentDisplayedFrame - HalfFrameCount;
			_endDisplayFrame = _timeline.CurrentDisplayedFrame + HalfFrameCount;

			foreach (var chip in Chips)
			{
				chip.UpdateVisuals();
			}
			//slow :(
			UpdateVisibleButtons();
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
		
		public void UpdateVisibleButtons()
		{
			//todo: only clear if the events are out of view.
			VisibleButtonEvents.Clear();
			for (long i = _startDisplayFrame;i< _endDisplayFrame; i++)
			{
				if (_timeline.TryGetFrame(i, out var input))
				{
					if (input.JumpButton != null)
					{
						if (!VisibleButtonEvents.Contains(input.JumpButton))
						{
							VisibleButtonEvents.Add(input.JumpButton);
						}
					}

					if (input.ArrowButton != null)
					{
						if (!VisibleButtonEvents.Contains(input.ArrowButton))
						{
							VisibleButtonEvents.Add(input.ArrowButton);
						}
					}
				}
			}
			//todo: we can get away with slow functions here because the truncated timeline won't show thousands of buttons... (I HOPE)
			for (int i = ButtonChips.Count - 1; i >= 0; i--)
			{
				Destroy(ButtonChips[i].gameObject);

				// if (ButtonChips[i].ReleaseFrame < StartDisplayFrame || ButtonChips[i].PressFrame > EndDisplayFrame)
				// {
				// 	ButtonChips.RemoveAt(i);
				// 	//todo: pooler
				// }
			}
			ButtonChips.Clear();

			for (int i = 0; i < VisibleButtonEvents.Count; i++)
			{
				//Create Button Chip.
				var chip = Instantiate(ButtonChipPrefab, ButtonChipParent);
				chip.Init(this,VisibleButtonEvents[i]);
				ButtonChips.Add(chip);
			}
		}

		private UIFrameChip CreateChip(int index)
		{
			var chip = Instantiate<UIFrameChip>(FrameChipPrefab, FrameTimelineParent);
			chip.SetIndex(index);
			chip.SetManager(this);
			return chip;
		}

		private void OnScrollbarValueChanged(float val)
		{
			Debug.Log($"Timeline Scrollbar set to {val}");
			// var size = _timeline.LastFrame();
			// _scrollbar.value = size*
		}

		public UIFrameChip GetLeftEdgeChip()
		{
			return Chips[0];
		}

		public UIFrameChip GetCurrentEdgeChip()
		{
			return  Chips[(int)HalfFrameCount];
		}
	}
}