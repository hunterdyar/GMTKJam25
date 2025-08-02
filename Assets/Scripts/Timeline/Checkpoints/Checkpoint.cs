using System.Collections.Generic;
using UnityEngine;

namespace GMTK
{
	public class Checkpoint
	{
		public CheckpointData[] _checkpointData;
		private int lastAdded = 0;
		public int Frame => _frame;
		private int _frame;
		public Checkpoint(int playbackFrame, int size)
		{
			_frame = playbackFrame;
			_checkpointData = new CheckpointData[size];
		}

		public void RegisterObject(CheckpointData data)
		{
			_checkpointData[lastAdded] = (data);
			lastAdded++;
		}

		public void RestoreCheckpoint()
		{
			foreach (var stored in _checkpointData)
			{
				stored.RestoreToCheckpoint();
			}
		}
		
	}
}