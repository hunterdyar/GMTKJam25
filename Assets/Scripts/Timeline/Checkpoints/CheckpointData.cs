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
	
	public class InteractableCheckpointData : CheckpointData
	{
		public bool _hasBeenInteractedWith;
		public bool _isInteracting;
		private readonly Interactable _interactable;
		public InteractableCheckpointData(Interactable interactable) : base(interactable)
		{
			_hasBeenInteractedWith = interactable.HasBeenInteractedWith;
			_isInteracting = interactable.IsInteracting;
			_interactable = interactable;
		}

		public override void RestoreToCheckpoint()
		{
			_interactable.HasBeenInteractedWith = _hasBeenInteractedWith;
			_interactable.IsInteracting = _isInteracting;
		}
	}

	public class GameManagerCheckpointData : CheckpointData
	{
		public GameState _state;
		private GameManager _gameManager;
		public GameManagerCheckpointData(GameManager manager) : base(manager)
		{
			_gameManager = manager;
			_state = manager.GameState;
		}

		override public void RestoreToCheckpoint()
		{
			_gameManager.SetGameState(_state);
		}
	}
}