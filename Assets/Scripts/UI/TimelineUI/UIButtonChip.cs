using System;
using GMTK;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Rendering;

namespace UI
{
	public class UIButtonChip : MonoBehaviour
	{
		private UITimelineManager _manager;
		public ButtonEvent ButtonEvent;
		[CanBeNull] public UIFrameChip _pressChip;
		[CanBeNull] public UIFrameChip _releaseChip;
		Vector3[] _corners = new Vector3[4];
		public void Update()
		{
			//update position.
			
			//scale self to span from press to release and look good and all that.
		}

		public void Init(UITimelineManager manager, ButtonEvent buttonEvent)
		{
			_manager = manager;
			ButtonEvent = buttonEvent;
			if (buttonEvent.PressFrame >= manager.StartDisplayFrame)
			{
				manager.TryGetFrameChip(buttonEvent.PressFrame, out _pressChip);
			}
			else
			{
				//off left edge of timeline!
			}

			if (buttonEvent.ReleaseFrame <= manager.EndDisplayFrame)
			{
				manager.TryGetFrameChip(buttonEvent.ReleaseFrame, out _releaseChip);
			}
			else
			{
				//off right edge of timeline
			}

			UpdateVisuals();
		}

		private void UpdateVisuals()
		{
			var r = transform as RectTransform;
			var leftChip = _pressChip;
			var rightChip = _releaseChip;
			bool offRightEdge = false;
			bool offLeftEdge = false;
			if (_pressChip == null)
			{
				offLeftEdge = true;
				leftChip = _manager.GetLeftEdgeChip();
			}

			if (_releaseChip == null)
			{
				offRightEdge = true;
				rightChip = _manager.GetCurrentEdgeChip();
			}
			
			if (leftChip != null && rightChip != null)
			{
				transform.position = Vector3.Lerp(leftChip.transform.position, rightChip.transform.position, 0.5f);
				var left = (leftChip.transform as RectTransform);
				var right = (rightChip.transform as RectTransform);
				var p = left.parent as RectTransform;
				r.anchoredPosition = new Vector2(Mathf.Lerp(left.anchoredPosition.x-left.rect.width/2, right.anchoredPosition.x+right.rect.width/2, 0.5f),r.anchoredPosition.y) - new Vector2(p.rect.width/2,0);
				r.sizeDelta = new Vector2(right.anchoredPosition.x-left.anchoredPosition.x,r.sizeDelta.y);
			}
			
			//change icon for when offleft and offright are true.
		}
	}
}