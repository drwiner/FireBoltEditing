using UnityEngine;
using UnityEngine.UI;
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
    public string storyPlanPath;
    public string cinematicModelPath;
    private float lastTickLogged;
    private int nextActionIndex;
    [HideInInspector]
    public bool pause = false;
    public Text debugText;

    /// <summary>
    /// FireBolt point of truth for time.  updated with but independent of time.deltaTime
    /// expressed in milliseconds
    /// </summary>
    public static float currentTime;

	// Use this for initialization
	void Awake () {
        ActorActionFactory.debugText = debugText;
        executingActions = new List<IActorAction>();
        aaq = ActorActionFactory.CreateStoryActions(storyPlanPath, cinematicModelPath);
        currentTime = 0;
        nextActionIndex = 0;
    }

    public void togglePause()
    {
        if (Time.timeScale < float.Epsilon)
            Time.timeScale = 1f;
        else
            Time.timeScale = 0f;
    }

    public void speedToggle()
    {
        Time.timeScale = (Time.timeScale + 1f) % 4;        
    }

    void Update()
    {
        currentTime += Time.deltaTime * 1000;
        logTicks();
        List<IActorAction> removeList = new List<IActorAction>();
        foreach (IActorAction actorAction in executingActions)
        {
            if (actorActionComplete(actorAction))
            {
                actorAction.Stop();
                //TODO need to recycle completed actions to somewhere for backward scrubbing
                removeList.Add(actorAction);
            }
        }
        foreach (IActorAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (aaq.Count > 0 && aaq[nextActionIndex].StartTick() <= currentTime)
        {
            IActorAction action = aaq[nextActionIndex];
            nextActionIndex++;
            if (action.Init())
                executingActions.Add(action);
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
        if (currentTime - lastTickLogged > 1000)
        {
            Debug.Log(currentTime);
            lastTickLogged = currentTime;
        }
    }

    bool actorActionComplete(IActorAction iaa)
    {
        return iaa.EndTick().HasValue && iaa.EndTick().Value < currentTime || iaa.EndTick() == null;
    }
}