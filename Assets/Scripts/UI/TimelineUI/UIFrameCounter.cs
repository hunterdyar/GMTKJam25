using System;
using GMTK;
using UnityEngine;

namespace UI
{
	public class UIFrameCounter : MonoBehaviour
	{
		private TMPro.TMP_Text _text;
		public Timeline Timeline;

		void Awake()
		{
			_text = GetComponent<TMPro.TMP_Text>();
		}
		private void Update()
		{
			_text.text = Timeline.CurrentDisplayedFrame.ToString();
		}
	}
}