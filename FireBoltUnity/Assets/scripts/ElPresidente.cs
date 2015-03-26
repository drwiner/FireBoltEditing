using UnityEngine;
using Impulse.v_0_1;
using ImpulsePlan = Impulse.v_0_1.Plan;
using System.Xml;
using System.IO;
using System.Collections;
using Assets.scripts;
using System.Collections.Generic;
using System;

public class ElPresidente : MonoBehaviour {

    ActorActionQueue aaq;
    List<IActorAction> executingActions;
    float currentTick;
    public string storyPlanPath;
    public string cinematicModelPath;

	// Use this for initialization
	void Start () {
        executingActions = new List<IActorAction>();
        aaq = ActorActionFactory.CreateStoryActions(storyPlanPath, cinematicModelPath);        
    }

	
	// Update is called once per frame
	void Update () {
        currentTick = Time.time;
        foreach (IActorAction actorAction in executingActions)
        {
            if (actorAction.endTick() < currentTick)
            {
                actorAction.Stop();
            }
        }
        while(aaq.Peek() != null && aaq.Peek().StartTick() >= currentTick)
        {
            IActorAction iaa = aaq.Pop();
            iaa.Init();
            executingActions.Add(iaa);
        }
	}

    void LateUpdate()
    {
        foreach (IActorAction actorAction in executingActions)
        {
            actorAction.Execute();            
        }
    }
}