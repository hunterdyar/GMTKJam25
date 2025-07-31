using UnityEngine;

namespace GMTK
{
	public class RBTimelineListener : TimelineListener
	{
		private Rigidbody _rigidbody;
		protected override void Awake()
		{
			base.Awake();
			_rigidbody = GetComponent<Rigidbody>();
		}

		protected override CheckpointData GetCheckpointData()
		{
			return new RigidbodyCPData(this, _rigidbody);
		}
	}
}