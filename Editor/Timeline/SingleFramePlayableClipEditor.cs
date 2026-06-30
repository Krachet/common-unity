using Com.Krackhet.Runtime.Timeline;
using UnityEditor.Timeline;
using UnityEngine.Timeline;

namespace Com.Krackhet.Editor.Timeline
{
    [CustomTimelineEditor(typeof(SingleFramePlayableClip))]
    public class SingleFramePlayableClipEditor : ClipEditor
    {
        public override void OnCreate(TimelineClip clip, TrackAsset track, TimelineClip clonedFrom)
        {
            base.OnCreate(clip, track, clonedFrom);

            if (clip.asset is SingleFramePlayableClip)
            {
                clip.duration = 1f/60f;
            }
        }
    }
}