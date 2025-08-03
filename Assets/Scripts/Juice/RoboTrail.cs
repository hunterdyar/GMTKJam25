using System;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;

namespace GMTK.Juice
{
	public class RoboTrail : TimelineListener
	{
		[HideInInspector] public int StartIndex;
		[HideInInspector] public int EndIndex;

		private int _visStart;
		private int _visEnd;
		public int Length;
		private LineRenderer _trail;
		private Vector3[] _points;

		protected override void Awake()
		{
			base.Awake();
			_points = new Vector3[_timeline.MaxFrame+1];
			_trail = GetComponent<LineRenderer>();
			_trail.positionCount = 0;
		}

		private void OnEnable()
		{
			Timeline.OnInput += OnInput;
			Timeline.OnCurrentDisplayFrameChanged += OnCurrentDisplayFrameChanged;
		}



		private void OnDisable()
		{
			Timeline.OnInput -= OnInput;
			Timeline.OnCurrentDisplayFrameChanged -= OnCurrentDisplayFrameChanged;

		}

		void Update()
		{
			int count = EndIndex - StartIndex;
			if (_trail.positionCount != count+1)
			{
				_trail.positionCount = count + 1;
			}
			
			if (_visStart != StartIndex || _visEnd != EndIndex)
			{
				for (int i = 0; i < count; i++)
				{
					_trail.SetPosition(i, _points[StartIndex + i]);
				}
				_visStart = StartIndex;
				_visEnd = EndIndex;
			}

			//always keep a last point on the player - every update, not every fixedupdate.
			_trail.SetPosition(count, transform.position);

		}

		private void OnCurrentDisplayFrameChanged(int frame)
		{
			EndIndex = frame;
			StartIndex = frame - Length;
			//clamp 0
			StartIndex = StartIndex < 0 ? 0 : StartIndex;
		}
		
		private void OnInput(int frame, GameInput arg2, bool arg3)
		{
			EndIndex = frame;
			StartIndex = frame - Length;
			//clamp 0
			StartIndex = StartIndex < 0 ? 0 : StartIndex;
			_points[frame] = transform.position;
		}

		
	}
}