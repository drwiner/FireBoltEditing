using UnityEngine;
using System.Collections;
using CommandPattern;
namespace CommandPattern {
//concrete
public class playAnimation : MonoBehaviour, ICommand{

	private Animator thisAnim;
	private int animationHash;

	public playAnimation(GameObject someGameObject, string animName) {
		thisAnim = someGameObject.GetComponent<Animator> ();
		animationHash = Animator.StringToHash (animName);
	}
	
	public void Execute () {
		thisAnim.SetTrigger (animationHash);
	}
	
}

}