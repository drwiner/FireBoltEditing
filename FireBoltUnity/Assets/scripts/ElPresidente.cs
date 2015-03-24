using UnityEngine;
using Impulse.v_0_1;
using ImpulsePlan = Impulse.v_0_1.Plan;
using System.Xml;
using System.IO;
using System.Collections;
using Assets.scripts;
using System.Collections.Generic;

public class ElPresidente : MonoBehaviour {

    ActorActionQueue aaq;
    List<IActorAction> executingActions;
    long currentTick;
    public string storyPlanPath;
    public string cinematicModelPath;

	// Use this for initialization
	void Start () {
        executingActions = new List<IActorAction>();
        aaq = ActorActionFactory.CreateStoryActions(storyPlanPath, cinematicModelPath);
    }

	
	// Update is called once per frame
	void Update () {
        foreach (IActorAction actorAction in executingActions)
        {
            if (actorAction.endTick() < currentTick)
            {
                actorAction.Stop();
            }
        }
        while(aaq.Peek().StartTick() >= currentTick)
        {
            IActorAction iaa = aaq.Dequeue();
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