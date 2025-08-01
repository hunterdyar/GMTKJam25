using System.Collections.Generic;
using UnityEngine;

namespace GMTK
{
	public class Checkpoint
	{
		public List<CheckpointData> _checkpointData = new List<CheckpointData>();

		public int Frame => _frame;
		private int _frame;
		public Checkpoint(int playbackFrame)
		{
			_frame = playbackFrame;
		}

		public void RegisterObject(CheckpointData data)
		{
			_checkpointData.Add(data);
		}

		public void RestoreCheckpoint()
		{
			Debug.Log($"Restore Checkpoint {Frame}");
			foreach (var stored in _checkpointData)
			{
				stored.RestoreToCheckpoint();
			}
		}
		
	}
}