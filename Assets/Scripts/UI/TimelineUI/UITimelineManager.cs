using System;
using System.Collections.Generic;
using GMTK;
using Unity.VisualScripting;
using UnityEngine;

namespace UI
{
	public class UITimelineManager : MonoBehaviour
	{
		public int StartDisplayFrame;
		public int EndDisplayFrame;

		public int TimelineLength = 100;
		public List<UIFrameChip> Chips;

		[Header("Scene Config")] public Transform FrameTimelineParent;

		public Timeline Timeline => _timeline;//public getter
		public Action OnPositionUpdate;

		[Header("Config")] [SerializeField] Timeline _timeline;
		public UIFrameChip FrameChipPrefab;
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
		}

		private void OnDisable()
		{
			Timeline.OnInput -= OnInput;
		}

		private void OnInput(long frame, GameInput input)
		{
			if (frame >= StartDisplayFrame && frame <= EndDisplayFrame)
			{
				int index = (int)(frame - StartDisplayFrame);
				//uh?
				if (index >= 0 && index < Chips.Count)
				{
					Chips[index].UpdateVisuals();
				}
			}
		}

		private UIFrameChip CreateChip(int index)
		{
			var chip = Instantiate<UIFrameChip>(FrameChipPrefab, FrameTimelineParent);
			chip.SetIndex(index);
			chip.SetManager(this);
			return chip;
		}
	}
}