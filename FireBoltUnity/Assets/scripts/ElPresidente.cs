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
    private float lastTickLogged;

	// Use this for initialization
	void Start () {
        executingActions = new List<IActorAction>();
        aaq = ActorActionFactory.CreateStoryActions(storyPlanPath, cinematicModelPath);        
    }

	
	// Update is called once per frame
	void Update () {
        currentTick = Time.time;
        logTicks();
        List<IActorAction> removeList = new List<IActorAction>();
        foreach (IActorAction actorAction in executingActions)
        {
            if (actorActionComplete(actorAction) )
            {
                actorAction.Stop();
                //TODO need to recycle completed actions to somewhere for backward scrubbing
                removeList.Add(actorAction);
            }
        }
        foreach (IActorAction iaa in removeList)
        {
            executingActions.Remove(iaa);
        }
        while(aaq.Peek() != null && aaq.Peek().StartTick() <= currentTick)
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

    void logTicks()
    {
        if (currentTick - lastTickLogged > 1)
        {
            Debug.Log(currentTick);
            lastTickLogged = currentTick;
        }
    }

    bool actorActionComplete(IActorAction iaa)
    {
        return iaa.EndTick().HasValue && iaa.EndTick().Value < currentTick || iaa.EndTick() == null;
    }
}