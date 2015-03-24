using UnityEngine;
using System.Collections;

namespace Assets.scripts {
//concrete
public class playLegacy : MonoBehaviour, IActorAction{

	private string thisAnim;

		public playLegacy(GameObject someGameObject, string animName) {
		thisAnim = animName;
	}
	
	public void Execute () {
		animation.Play (thisAnim);
        Time.d      
	}
	
}

}