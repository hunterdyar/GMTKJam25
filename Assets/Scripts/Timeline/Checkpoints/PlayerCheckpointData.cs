using UnityEngine;

namespace GMTK
{
	public class PlayerCheckpointData : CheckpointData
	{
		private PlayerMovement _player;
		private Rigidbody _rb;
		
		//
		public Vector3 Position;
		public Quaternion Rotation;
		public Vector3 LinearVelocity;
		public Vector3 AngularVelocity;
		public Vector3 InputDir;
		public float MoveSpeed;

		public PlayerCheckpointData(TimelineListener timelineListener, Rigidbody rb, PlayerMovement player) : base(timelineListener)
		{
			_player = player;
			_rb = rb;
			Position = _rb.transform.position;
			Rotation = _rb.transform.rotation;
			LinearVelocity = _rb.linearVelocity;
			AngularVelocity = _rb.angularVelocity;
			InputDir = _player.inputDirection;
			MoveSpeed = _player.moveSpeed;
		}

		public override void RestoreToCheckpoint()
		{
			_rb.transform.position = Position;
			_rb.transform.rotation = Rotation;
			_rb.linearVelocity = LinearVelocity;
			_rb.angularVelocity = AngularVelocity;
			_player.inputDirection = InputDir;
			_player.moveSpeed = MoveSpeed;
		}

	}
}