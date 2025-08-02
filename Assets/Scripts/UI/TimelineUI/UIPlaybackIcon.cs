using GMTK;
using UnityEngine;
using UnityEngine.UI;

namespace UI
{
	public class UIPlaybackIcon : MonoBehaviour
	{
		public bool ActiveIsOppositePlaying;
		private Image[] _images;
		public Color PlaybackColor;
		public Color ScrubbingColor;
		public Color RecordingColor;
		public Color PrimedColor;

		void Awake()
		{
			//yes I am doing this instead of updating the scene it's late and im getting stuff done.
			_images = GetComponentsInChildren<Image>();
		}
		private void OnEnable()
		{
			TimelineRunner.OnPlaybackChange += OnPlaybackChange;
			TimelineRunner.OnStateChange += OnStateChange;
		}

		

		private void OnDisable()
		{
			TimelineRunner.OnPlaybackChange -= OnPlaybackChange;
			TimelineRunner.OnStateChange -= OnStateChange;
		}
		

		private void OnPlaybackChange(bool playing)
		{
			foreach (Transform child in transform)
			{
				child.gameObject.SetActive(ActiveIsOppositePlaying ? playing : !playing);
			}
		}

		private void OnStateChange(RunnerControlState state)
		{
			switch (state)
			{
					case RunnerControlState.Playback:
						SetColor(PlaybackColor);
						break;
					case RunnerControlState.Scrubbing:
						SetColor(ScrubbingColor);
						break;
					case RunnerControlState.Recording:
						SetColor(RecordingColor);
						break;
					case RunnerControlState.WaitingToRecord:
						SetColor(PrimedColor);
						break;
			}
		}

		void SetColor(Color color)
		{
			foreach (Image image in _images)
			{
				image.color = color;
			}
		}
		
	}
}