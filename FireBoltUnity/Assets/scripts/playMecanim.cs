using UnityEngine;
using System.Collections;

namespace Assets.scripts {
//concrete
public class playMecanim : MonoBehaviour, IActorAction{

	private Animator thisAnim;
	private int animationHash;
    private long startTick;

		public playMecanim(GameObject someGameObject, string animName) {
		thisAnim = someGameObject.GetComponent<Animator> ();
		animationHash = Animator.StringToHash (animName);
	}
	
	public void Execute () {
		thisAnim.SetTrigger (animationHash);
	}

    public long startTick()
    {
        return startTick;
    }
	
}

}