using System;
using Timeline;
using UnityEngine;

public class TimelineListener : MonoBehaviour
{
    public Timeline.Timeline _timeline;


    protected virtual void Awake()
    {
        _timeline.AddTimelineListener(this);
    }

    private void OnDestroy()
    {
        _timeline.RemoveTimelineListener(this);
    }

    public void SaveCurrentSelfToCheckpoint(ref Checkpoint checkpoint)
    {
        checkpoint.RegisterObject(GetCheckpointData());
    }

    protected virtual CheckpointData GetCheckpointData()
    {
        return new TransformData(this)
        {
            
            Position = transform.position,
            Rotation = transform.rotation
        };
    }
}
