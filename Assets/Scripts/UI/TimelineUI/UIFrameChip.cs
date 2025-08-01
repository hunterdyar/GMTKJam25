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
			long realFrame = _timelineManager.StartDisplayFrame + _relativeIndex;
			if (realFrame > _timelineManager.EndDisplayFrame || realFrame < _timelineManager.StartDisplayFrame )
			{
				// _image.enabled = false;
				_image.color = Color.black;

				return;
			}

			if (realFrame < 0 || realFrame > _timelineManager.Timeline.LastFrame())
			{
				_image.color = Color.black;
				return;
			}

			return;
			if (_timelineManager.Timeline.TryGetFrame(realFrame, out var inputs))
			{
				_image.enabled = true;
				//set active
				//show icons
				if (inputs.Any())
				{
					_image.color = Color.blueViolet;
				}
				else
				{
					_image.color = Color.white;
				}
			}
			else
			{
				_image.enabled = true;
				_image.color = Color.white;
			}

			if (realFrame == _timelineManager.Timeline.CurrentDisplayedFrame)
			{
				_image.color = Color.yellow;
			}
		}
	}
}