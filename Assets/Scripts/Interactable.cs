using System;
using GMTK;
using UnityEngine;

namespace DefaultNamespace
{
	public class Interactable : TimelineListener
	{
		public Timeline _timeline;

		public bool HasBeenInteractedWith;
		public bool IsInteracting;
		//other?
		private BoxCollider _box;
		private Collider[] _castResults = new Collider[10];
		protected override void Awake()
		{
			base.Awake();
			Timeline.OnInput += OnInput;
			
		}

		private void OnInput(long arg1, GameInput arg2, bool arg3)
		{
			//if overlap.... set some bool.
			var size = Physics.OverlapBoxNonAlloc(_box.bounds.center, _box.bounds.extents, _castResults);
			for (int i = 0; i < size; i++)
			{
				var player = _castResults[i]?.GetComponent<PlayerMovement>();
				if (player != null)
				{
					if (!HasBeenInteractedWith)
					{
						HasBeenInteractedWith = true;
						OnFirstInteraction();
					}

					if (!IsInteracting)
					{
						IsInteracting = true;
						OnInteraction();
					}
				}
				else
				{
					IsInteracting = false;
				}

				break;
			}
		}

		public virtual void OnFirstInteraction()
		{
			//... 	
		}

		public virtual void OnInteraction()
		{
			
		}

		protected override CheckpointData GetCheckpointData()
		{
			return new InteractableCheckpointData(this)
			{
				_hasBeenInteractedWith = HasBeenInteractedWith,
				_isInteracting  = IsInteracting
			};
		}
	}
}