using System.Collections.Generic;

namespace GMTK
{
	public class Checkpoint
	{
		public List<CheckpointData> _checkpointData = new List<CheckpointData>();

		public long Frame => _frame;
		private long _frame;
		public Checkpoint(long playbackFrame)
		{
			_frame = playbackFrame;
		}

		public void RegisterObject(CheckpointData data)
		{
			_checkpointData.Add(data);
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