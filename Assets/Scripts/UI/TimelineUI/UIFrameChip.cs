using System;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UIFrameChip : MonoBehaviour
	{
		private Image _image;
		private int _relativeIndex = 0;
		private UITimelineManager _timelineManager;

		void Awake()
		{
			_image = GetComponent<Image>();
		}
		public void SetIndex(int index)
		{
			_relativeIndex = index;
		}

		public void SetManager(UITimelineManager manager)
		{
			_timelineManager = manager;
			_timelineManager.OnPositionUpdate += UpdateVisuals;
		}

		public void UpdateVisuals()
		{
			//if is playbackframe, make special color!
			
			// long realFrame = _timelineManager.StartDisplayFrame + _relativeIndex;
			// if (realFrame > _timelineManager.EndDisplayFrame)
			// {
			// 	_image.enabled = false;
			// 	return;
			// }
			// if (_timelineManager.Timeline.TryGetFrame(realFrame, out var inputs))
			// {
			// 	//set active
			// 	//show icons
			// 	_image.color = Color.blueViolet;
			// 	_image.enabled = true;
			// 	
			// }
			// else
			// {
			// 	_image.enabled = true;
			// 	_image.color = Color.white;
			// }
		}
	}
}