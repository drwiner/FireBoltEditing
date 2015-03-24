using UnityEngine;
using System.Collections;
using CommandPattern;

public class testActionExecution : MonoBehaviour {

	public GameObject someGameObject;
	// Use this for initialization
	void Start () {
		playAnimation newPlayAnim = new playAnimation (someGameObject, "hacking");
		actionExecution.play (newPlayAnim);
	}
	
	// Update is called once per frame
	void Update () {


	}
}
