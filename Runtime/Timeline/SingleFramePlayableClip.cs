using UnityEngine;
using UnityEngine.Playables;

namespace Com.Krackhet.Runtime.Timeline
{
    public class SingleFramePlayableClip : PlayableAsset 
    {
        public override double duration => 10.0 / 60.0;

        public override Playable CreatePlayable(PlayableGraph graph, GameObject owner)
        {
            var playable = ScriptPlayable<SingleFramePlayableBehaviour>.Create(graph);
            return playable;
        }
    }
}