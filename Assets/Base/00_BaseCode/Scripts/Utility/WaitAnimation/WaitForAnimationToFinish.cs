using UnityEngine;
namespace Gks
{
    public class WaitForAnimationToFinish : CustomYieldInstruction
    {

        private readonly string animationName;

        private readonly Animator animator;
        private readonly int layerIndex;


        private AnimatorStateInfo StateInfo => animator.GetCurrentAnimatorStateInfo(layerIndex);

        private bool CorrectAnimationIsPlaying => StateInfo.IsName(animationName);

        private bool AnimationIsDone => StateInfo.normalizedTime >= 1;

        public override bool keepWaiting => IsRun();
        //public override bool keepWaiting => true;

        private bool isWait = true;
        private bool IsRun()
        {
            if (isWait)
            {
                if (CorrectAnimationIsPlaying)
                {
                    isWait = false;
                }
            }
            else
            {
                return !AnimationIsDone;
            }

            return true;
        }

        /// <summary>
        ///     Creates a new yield-instruction
        /// </summary>
        /// <param name="animator">The animator to track</param>
        /// <param name="animationName">The name of the animation</param>
        /// <param name="layerIndex">The layer the animation is playing on</param>
        public WaitForAnimationToFinish(Animator animator, string animationName, int layerIndex = 0)
        {
            this.animator = animator;
            this.layerIndex = layerIndex;
            this.animationName = animationName;
        }

    }

}