using UnityEngine.Playables;

namespace Com.Krackhet.Runtime.Timeline
{
    public class SingleFramePlayableBehaviour : PlayableBehaviour
    {
        private int _frameCount;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            if (_frameCount >= 1) return;
            _frameCount++;
            OnFirstFramePlay();
        }

        protected virtual void OnFirstFramePlay()
        {
        }
    }
}