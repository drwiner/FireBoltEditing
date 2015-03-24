using UnityEngine;
using System.Collections;
using CommandPattern;
namespace CommandPattern {
//concrete
public class playLegacy : MonoBehaviour, ICommand{

	private string thisAnim;

		public playLegacy(GameObject someGameObject, string animName) {
		thisAnim = animName;
	}
	
	public void Execute () {
		animation.Play (thisAnim);
	}
	
}

}