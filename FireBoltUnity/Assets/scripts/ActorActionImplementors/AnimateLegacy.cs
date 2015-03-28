using UnityEngine;
using System.Collections;

namespace Assets.scripts {
//concrete
public class AnimateLegacy : ActionDecorator{

	private string thisAnim;

		public AnimateLegacy(float startTick, float? endTick, string actorName,  
            IActorAction nestedAction, string animName) :
            base(startTick,endTick,nestedAction) {
		thisAnim = animName;
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
        //GetComponent<Animation>().Play (thisAnim); 
        throw new System.NotImplementedException();
    }
}

}