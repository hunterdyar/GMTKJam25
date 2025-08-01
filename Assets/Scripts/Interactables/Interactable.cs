using System;
using GMTK;
using UnityEngine;

namespace GMTK
{
	public class Interactable : TimelineListener
	{
		public bool HasBeenInteractedWith;
		public bool IsInteracting;
		//other?
		private BoxCollider _box;
		private Collider[] _castResults = new Collider[10];
		protected override void Awake()
		{
			base.Awake();
			Timeline.OnInput += OnInput;
			_box = GetComponent<BoxCollider>();
		}

		private void OnEnable()
		{
			Timeline.OnInput += OnInput;
		}

		private void OnDisable()
		{
			Timeline.OnInput -= OnInput;
		}

		private void OnInput(long arg1, GameInput arg2, bool arg3)
		{
			//if overlap.... set some bool.
			bool noPlayer = true;
			var size = Physics.OverlapBoxNonAlloc(_box.bounds.center, _box.bounds.extents, _castResults);
			for (int i = 0; i < size; i++)
			{
				var player = _castResults[i]?.GetComponent<PlayerMovement>();
				if (player != null)
				{
					noPlayer = false;
					if (!HasBeenInteractedWith)
					{
						HasBeenInteractedWith = true;
						OnFirstInteraction(player);
					}

					if (!IsInteracting)
					{
						IsInteracting = true;
						OnInteraction(player);
					}
					break;
				}
			}

			if (noPlayer)
			{
				IsInteracting = false;
			}
		}

		public virtual void OnFirstInteraction(PlayerMovement player)
		{
			//... 	
		}

		public virtual void OnInteraction(PlayerMovement player)
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