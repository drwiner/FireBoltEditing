using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    //concrete
    public class AnimateLegacy : ActionDecorator
    {

        private string animName;
        private string actorName;
        public AnimateLegacy(float startTick, float? endTick, string actorName,
            IActorAction nestedAction, string animName) :
            base(startTick, endTick, nestedAction)
        {
            this.animName = animName;
            this.actorName = actorName;
            

        }

        public override void execute()
        {

        }

        public override void stop()
        {
            throw new System.NotImplementedException();
        }

        public override void init()
        {
            //look up actor, get component and play animation on it
            GameObject.Find(actorName).GetComponent<Animation>().Play(animName); 
        
        }
    }
}