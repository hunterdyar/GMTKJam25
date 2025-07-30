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
		public long StartDisplayFrame;
		public long EndDisplayFrame;

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
		
		[SerializeField] Scrollbar _scrollbar;
		void Start()
		{
			Chips = new List<UIFrameChip>();
			for (int i = 0; i < TimelineLength; i++)
			{
				Chips.Add(CreateChip(i));
			}
			
			OnPositionUpdate?.Invoke();
			if (_scrollbar == null)
			{
				_scrollbar = GetComponentInChildren<Scrollbar>();	
				_scrollbar.onValueChanged.AddListener(OnScrollbarValueChanged);
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
			//slow :(
			UpdateVisibleButtons();
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
			for (long i = StartDisplayFrame;i<EndDisplayFrame; i++)
			{
				if (_timeline.TryGetFrame(i, out var input))
				{
					if (!VisibleButtonEvents.Contains(input.JumpButton))
					{
						VisibleButtonEvents.Add(input.JumpButton);
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
		
		public void SetStartAndEnd(long newStart, long newEnd)
		{
			if (newStart != StartDisplayFrame || newEnd != EndDisplayFrame)
			{
				StartDisplayFrame = newStart;
				EndDisplayFrame = newEnd;
				UpdateScrollbarByValues();
				UpdateVisuals();
			}
		}

		private void UpdateScrollbarByValues()
		{
			//todo: none of this works
			var size = _timeline.LastFrame();
			if (size == 0)
			{
				_scrollbar.size = 1;
				return;
			}
			var visibleWidth = (EndDisplayFrame - StartDisplayFrame) / size;
			_scrollbar.size = visibleWidth;
			var middleFrame = StartDisplayFrame + (StartDisplayFrame-EndDisplayFrame)/2;
			_scrollbar.value = 1-(middleFrame / (float)size);
		}

		private void OnScrollbarValueChanged(float val)
		{
			// var size = _timeline.LastFrame();
			// _scrollbar.value = size*
		}

		public UIFrameChip GetLeftEdgeChip()
		{
			return Chips[0];
		}

		public UIFrameChip GetRightEdgeChip()
		{
			return  Chips[^1];
		}
	}
}