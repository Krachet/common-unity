using Com.Krackhet.Runtime.Timeline;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Com.Krackhet.Editor.Timeline
{
    [CustomTimelineEditor(typeof(SingleFramePlayableClip<SingleFramePlayableBehaviour>))]
    public class SingleFramePlayableClipEditor : ClipEditor
    {
        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            base.OnCreate(clip, track, clonedFrom);

            if (clip.asset is SingleFramePlayableClip<SingleFramePlayableBehaviour>)
            {
                clip.duration = 10f/60f;
            }
        }
    }
}