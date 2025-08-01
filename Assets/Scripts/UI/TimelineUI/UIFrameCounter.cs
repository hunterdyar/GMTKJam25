using System;
using GMTK;
using UnityEngine;

namespace UI
{
	public class UIFrameCounter : MonoBehaviour
	{
		private TMPro.TMP_Text _text;
		public Timeline Timeline;
		private long _displayed = long.MaxValue;//some impossible value to force a draw on first update.
		void Awake()
		{
			_text = GetComponent<TMPro.TMP_Text>();
		}
		private void Update()
		{
			if (_displayed == Timeline.CurrentDisplayedFrame)
			{
				return;
			}
			
			if (Timeline.CurrentDisplayedFrame >= 0)
			{
				_text.text = Timeline.CurrentDisplayedFrame.ToString();
				_displayed = Timeline.CurrentDisplayedFrame;
			}
			else
			{
				_text.text = 0.ToString();
				_displayed = 0;
			}
		}
	}
}