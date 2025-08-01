using System;
using UnityEngine;

namespace GMTK
{
	public class Collectable : Interactable
	{
		private MeshRenderer _meshRenderer;
		//for tracking when we need to switch visuals, not a source of truth.
		private bool _hasBeenCollected;

		protected override void Awake()
		{
			base.Awake();
			_meshRenderer = GetComponent<MeshRenderer>();
		}

		void Start()
		{
			SetCollected(false);
		}

		private void Update()
		{
			if (HasBeenInteractedWith != _hasBeenCollected)
			{
				SetCollected(HasBeenInteractedWith);
			}
		}

		private void SetCollected(bool collected)
		{
			_hasBeenCollected = collected;
			_meshRenderer.enabled = !collected;
		}
	}
}