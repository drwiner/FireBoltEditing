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
	
	private void execute () {
		   
	}

    private void stop()
    {
        throw new System.NotImplementedException();
    }

    private void init()
    {
        //look up actor, get component and play animation on it
        //GetComponent<Animation>().Play (thisAnim); 
        throw new System.NotImplementedException();
    }
}

}