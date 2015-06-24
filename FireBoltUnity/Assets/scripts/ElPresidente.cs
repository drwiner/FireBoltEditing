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
    List<Keyframe> keyFrames;
    public string storyPlanPath;
    public string cinematicModelPath;
    public float keyFrameFrequency=10000; //every ? milliseconds
    private float lastTickLogged;
    private int nextActionIndex;
    private float totalTime;
    private bool pause = false;
    public Text debugText;
	public float myTime;

    /// <summary>
    /// FireBolt point of truth for time.  updated with but independent of time.deltaTime
    /// expressed in milliseconds
    /// </summary>
    public static float currentTime;

	// Use this for initialization
	void Awake () {
        ActorActionFactory.debugText = debugText;
        executingActions = new List<IActorAction>();
        keyFrames = new List<Keyframe>();
        aaq = ActorActionFactory.CreateStoryActions(storyPlanPath, cinematicModelPath);
        currentTime = 0;
        nextActionIndex = 0;
        //find total time for execution. not sure how to easily find this without searching a lot of actions
        //totalTime = yeah 
        //TODO move story load into el presidente

		float at = 0;
		KeyFrame current;
		foreach (IActorAction aa in aaq) 
		{
			if (aa.StartTick() > at + keyFrameFrequency)
			{

			}
		}

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

    public void setTime(float f)
    {
        
        //find previous keyframe from calculated time
        debugText.text = (f * totalTime).ToString();
        //assign above to currentTime 
        //clear executing actions
        //enable/disable characters & reposition at start locations
        //set nextAction index
    }

    void Update()
    {

        currentTime += Time.deltaTime * 1000;
		myTime = currentTime;  
        logTicks();
        List<IActorAction> removeList = new List<IActorAction>();
        foreach (IActorAction actorAction in executingActions)
        {
            if (actorActionComplete(actorAction) || actorAction.StartTick() > currentTime)
            {
                actorAction.Stop();
                removeList.Add(actorAction);
            }
        }
        foreach (IActorAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (nextActionIndex < aaq.Count && aaq[nextActionIndex].StartTick() <= currentTime)
        {
            IActorAction action = aaq[nextActionIndex];
            nextActionIndex++;
            if (action.Init())
			{
				if (!actorActionComplete(action))
                    executingActions.Add(action);
				else
					action.Stop();
			}
        }
    }

    void LateUpdate()
    {
        foreach (IActorAction actorAction in executingActions)
        {
            actorAction.Execute();
        }
    }

	public void goTo(float time)
	{
		if (time < currentTime)
		{
			while (nextActionIndex >= 0 && aaq[nextActionIndex].StartTick() > time)
			{
				aaq[nextActionIndex].Undo();
				nextActionIndex--;
			}
		}
		currentTime = time;
	}

	public void goToRel(float time)
	{
		goTo(currentTime + time);
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