using System;
using GMTK;
using JetBrains.Annotations;
using UnityEngine;

namespace UI
{
	public class UIButtonChip : MonoBehaviour
	{
		private UITimelineManager _manager;
		public ButtonEvent ButtonEvent;
		private RectTransform _rectTransform;
		private RectTransform _parent;

		private void Awake()
		{
			_rectTransform = GetComponent<RectTransform>();
			_parent = transform.parent as RectTransform;
		}

		public void Update()
		{
			UpdateVisuals();
			
		}

		public void Init(UITimelineManager manager, ButtonEvent buttonEvent)
		{
			_manager = manager;
			ButtonEvent = buttonEvent;
			UpdateVisuals();
		}

		private void UpdateVisuals()
		{
			var r = _rectTransform;

			if (_manager == null)
			{
				gameObject.SetActive(false);
				return;
			}

			if (ButtonEvent.PressFrame >= ButtonEvent.ReleaseFrame && ButtonEvent.ReleaseFrame != -1)
			{
				_manager.InvalidateButton(ButtonEvent, this);
				return;
			}
			var (left,right, visible) = _manager.GetFramePosition(ButtonEvent);
			if (!visible)
			{
				//should be hidden!
			}
			if (right < left)
			{
				//Debug.LogWarning("reversed?");
			}
			if (left != right || ButtonEvent.PressFrame == ButtonEvent.ReleaseFrame || ButtonEvent.ReleaseFrame == -1)
			{
			}
			else
			{
				//off the edge, i suppose!
				//gameObject.SetActive(false);
				return;
			}

			var yShift = ButtonEvent.Button == Buttons.Jump ? -_parent.rect.height * .2f : _parent.rect.height * .3f;

			r.anchoredPosition = new Vector2(_parent.rect.min.x+Mathf.Lerp(left, right, 0.5f), _parent.anchoredPosition.y-yShift);
			//move % up or down depending on if arrows or jump.
			//sorry for the magic numbers :p
			r.sizeDelta = new Vector2(right-left,r.sizeDelta.y);
			
			//change icon for when offleft and offright are true.
		}
	}
}