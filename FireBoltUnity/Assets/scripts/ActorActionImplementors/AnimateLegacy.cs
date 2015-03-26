using UnityEngine;
using System.Collections;

namespace Assets.scripts {
//concrete
public class AnimateLegacy : MonoBehaviour, IActorAction{

	private string thisAnim;

		public AnimateLegacy(GameObject someGameObject, string animName) {
		thisAnim = animName;
	}
	
	public void Execute () {
		GetComponent<Animation>().Play (thisAnim);    
	}


    public long StartTick()
    {
        throw new System.NotImplementedException();
    }

    public long endTick()
    {
        throw new System.NotImplementedException();
    }

    public void Stop()
    {
        throw new System.NotImplementedException();
    }

    public void Init()
    {
        throw new System.NotImplementedException();
    }
}

}