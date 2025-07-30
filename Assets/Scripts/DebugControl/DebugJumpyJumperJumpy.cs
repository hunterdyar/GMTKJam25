using System;
using GMTK;
using UnityEngine;

namespace DebugControl
{
	public class DebugJumpyJumperJumpy : MonoBehaviour
	{
		private Rigidbody _rigidbody;
		public Timeline Timeline;
		
		private void Awake()
		{
			_rigidbody = GetComponent<Rigidbody>();
		}

		private void OnEnable()
		{
			Timeline.OnInput += OnInput;
		}

		private void OnDisable()
		{
			Timeline.OnInput -= OnInput;
		}
		private void OnInput(long frame, GameInput input)
		{
			if (input == GameInput.None)
			{
				return;
			}
			
			if ((input & GameInput.AButton) > 0)
			{
				_rigidbody.AddForce(Vector3.up*3, ForceMode.Impulse);
			}
		}

	
	}
}