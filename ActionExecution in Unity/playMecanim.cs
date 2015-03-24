using UnityEngine;
using System.Collections;
using CommandPattern;
namespace CommandPattern {
//concrete
public class playMecanim : MonoBehaviour, ICommand{

	private Animator thisAnim;
	private int animationHash;

		public playMecanim(GameObject someGameObject, string animName) {
		thisAnim = someGameObject.GetComponent<Animator> ();
		animationHash = Animator.StringToHash (animName);
	}
	
	public void Execute () {
		thisAnim.SetTrigger (animationHash);
	}
	
}

}