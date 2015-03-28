using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    public class AnimateMecanim : ActionDecorator
    {
        private Animator thisAnim;
        private int animationHash;
        private long startTick;

        public AnimateMecanim(float startTick, float? endTick, string actorName,
            IActorAction nestedAction, string animName) :
            base(startTick, endTick, nestedAction)
        {


        }

        public override void execute()
        {
            thisAnim.SetTrigger(animationHash);
        }

        public override void stop()
        {
            throw new System.NotImplementedException();
        }

        public override void init()
        {
            //look up actor, get component and play animation on it
            //thisAnim = someGameObject.GetComponent<Animator> ();
            //animationHash = Animator.StringToHash(animName);
            throw new System.NotImplementedException();
        }
    }

}