using UnityEngine;

namespace GMTK
{
	public abstract class CheckpointData
	{
		public TimelineListener Listener;

		public abstract void RestoreToCheckpoint();

		public CheckpointData(TimelineListener timelineListener)
		{
			Listener = timelineListener;
		}
		
	}

	public class TransformData : CheckpointData
	{
		
		public Vector3 Position;
		public Quaternion Rotation;


		public TransformData(TimelineListener timelineListener) : base(timelineListener)
		{
		}

		public override void RestoreToCheckpoint()
		{
			Listener.transform.position = Position;
			Listener.transform.rotation = Rotation;
		}
		
	}

	public class RigidbodyCPData : CheckpointData
	{
		private Rigidbody _rb;
		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 LinearVelocity;
		public Vector3 AngularVelocity;


		public RigidbodyCPData(TimelineListener timelineListener, Rigidbody rb) : base(timelineListener)
		{
			_rb = rb;
			Position = _rb.position;
			Rotation = _rb.rotation;
			LinearVelocity = _rb.linearVelocity;
			AngularVelocity = _rb.angularVelocity;
		}

		public override void RestoreToCheckpoint()
		{
			_rb.position = Position;
			_rb.rotation = Rotation;
			_rb.linearVelocity = LinearVelocity;
			_rb.angularVelocity = AngularVelocity;
		}

	}
}