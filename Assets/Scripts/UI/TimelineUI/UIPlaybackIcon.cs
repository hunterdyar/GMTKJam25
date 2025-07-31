using GMTK;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UIPlaybackIcon : MonoBehaviour
	{
		public bool ActiveIsOppositePlaying;
		private void OnEnable()
		{
			TimelineRunner.OnPlaybackChange += OnPlaybackChange;
		}

		private void OnDisable()
		{
			TimelineRunner.OnPlaybackChange -= OnPlaybackChange;
		}

		private void OnPlaybackChange(bool state)
		{
			SetStateIcon(state);
		}

		private void SetStateIcon(bool playing)
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(ActiveIsOppositePlaying ? playing : !playing);
			}
		}
	}
}