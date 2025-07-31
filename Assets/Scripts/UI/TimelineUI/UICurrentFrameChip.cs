using System;
using UnityEngine;

namespace UI
{
	public class UICurrentFrameChip : MonoBehaviour
	{
		private UITimelineManager _manager;
		private RectTransform _rectTransform;

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();//transform as RectTransform :p
		}

		public void Init(UITimelineManager manager)
		{
			_manager = manager;
		}

		public void Update()
		{
			if (_manager != null)
			{
				var c = _manager.Timeline.CurrentDisplayedFrame;
				if (_manager.TryGetFrameChip(c, out var chip))
				{
					_rectTransform.anchoredPosition = (chip.transform as RectTransform).anchoredPosition;
				}
			}
		}
	}
}