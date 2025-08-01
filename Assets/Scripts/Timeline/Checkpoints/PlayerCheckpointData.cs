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
		public bool Grounded;
		public bool IsJump;
		public float MoveSpeed;
		public float JumpBufferTimer;
		public float CoyoteTimer;

		public PlayerCheckpointData(TimelineListener timelineListener, Rigidbody rb, PlayerMovement player) : base(timelineListener)
		{
			_player = player;
			_rb = rb;
			Position = _rb.position;
			Rotation = _rb.rotation;
			LinearVelocity = _rb.linearVelocity;
			Debug.Log("player lvy get:"+_rb.linearVelocity.y);
			AngularVelocity = _rb.angularVelocity;
			InputDir = _player.inputDirection;
			MoveSpeed = _player.moveSpeed;
			Grounded = _player.isGrounded;
			IsJump = _player.jump;
			JumpBufferTimer = _player.jumpBufferTimer;
			CoyoteTimer = _player.coyoteTimer;
		}

		public override void RestoreToCheckpoint()
		{
			_rb.position = Position;
			_rb.rotation = Rotation;
			_rb.linearVelocity = LinearVelocity;
			Debug.Log("player lvy set:" + LinearVelocity.y);
			_rb.angularVelocity = AngularVelocity;
			_player.inputDirection = InputDir;
			_player.moveSpeed = MoveSpeed;
			_player.isGrounded = Grounded;
			_player.jump = IsJump;
			_player.jumpBufferTimer = JumpBufferTimer;
			_player.coyoteTimer = CoyoteTimer;
		}

	}
}