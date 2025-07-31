using UnityEngine;

namespace GMTK
{
	public class PlayerTimelineListener : TimelineListener
	{
		private Rigidbody _rigidbody;
		private PlayerMovement _player;

		protected override void Awake()
		{
			base.Awake();
			_rigidbody = GetComponent<Rigidbody>();
			_player = GetComponent<PlayerMovement>();
		}

		protected override CheckpointData GetCheckpointData()
		{
			return new PlayerCheckpointData(this, _rigidbody, _player);
		}
	}
}