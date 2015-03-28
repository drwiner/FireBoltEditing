using UnityEngine;
using System.Collections;
using CommandPattern;
namespace CommandPattern {
//concrete
public class playMecanim//: ICommand
{

	private Animator thisAnim;
	private static int animationHash = Animator.StringToHash ("trigger");

	public playMecanim(GameObject someGameObject, string animName) {
		
		thisAnim = someGameObject.GetComponent<Animator> ();
		AnimatorOverrideController myOverride = new AnimatorOverrideController();
		myOverride.runtimeAnimatorController = thisAnim.runtimeAnimatorController;
		AnimationClip newAnim = Resources.Load ("Animations/" + animName) as AnimationClip;
		
		if (!newAnim) {
			Debug.LogError("Missing animation asset");
		} 
		myOverride ["Idle_Glance"] = newAnim;

	}
	
	public void Execute () {
		thisAnim.SetTrigger (animationHash);
	}
	
}

}