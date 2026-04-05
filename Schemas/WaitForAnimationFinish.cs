using UnityEngine;

namespace Com.Krackhet.Schemas
{
    public class WaitForAnimationFinish : CustomYieldInstruction
    {
        private int layerIndex;
        private Animator animator;
        private string animationName;
        public override bool keepWaiting
        {
            get
            {
                AnimatorStateInfo animatorStateInfo = animator.GetCurrentAnimatorStateInfo(layerIndex);
                return animatorStateInfo.IsName(animationName) && animatorStateInfo.normalizedTime < 1;
            }
        }
        public WaitForAnimationFinish(Animator animator, string animationName, int layerIndex = 0)
        {
            this.animator = animator;
            this.layerIndex = layerIndex;
            this.animationName = animationName;
        }
    }
}