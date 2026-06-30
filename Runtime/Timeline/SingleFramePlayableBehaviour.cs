using UnityEngine.Playables;

namespace Com.Krackhet.Runtime.Timeline
{
    public class SingleFramePlayableBehaviour : PlayableBehaviour
    {
        private int _frameCount;

        public override void OnBehaviourPlay(Playable playable, FrameData info)
        {
            base.OnBehaviourPlay(playable, info);
            _frameCount = 0;
        }

        public override void ProcessFrame(Playable playable, FrameData info, object playerData)
        {
            base.ProcessFrame(playable, info, playerData);
            _frameCount++;
            if (_frameCount == 1)
            {
                OnFirstFramePlay();
            }
        }

        protected virtual void OnFirstFramePlay() { }
    }
}