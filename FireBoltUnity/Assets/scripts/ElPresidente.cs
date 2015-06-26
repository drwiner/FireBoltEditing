using UnityEngine;
using UnityEngine.UI;
using Impulse.v_1_336;
using System.Xml;
using System.IO;
using System.Collections;
using Assets.scripts;
using System.Collections.Generic;
using System;
using UintT = Impulse.v_1_336.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;

public class ElPresidente : MonoBehaviour {

    ActorActionQueue actorActionQueue;
    ActorActionQueue cameraActionQueue;
    List<IActorAction> executingActions;
    List<Keyframe> keyFrames;
    public string storyPlanPath;
    public string cinematicModelPath;
    public string cameraPlanPath;
    public float keyFrameFrequency=10000; //every ? milliseconds
    private float lastTickLogged;
    private int nextActionIndex;
    private float totalTime;
    private bool pause = false;
    public Text debugText;
	public float myTime;
    private AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story;

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
        loadStructuredImpulsePlan(storyPlanPath);
        actorActionQueue = ActorActionFactory.CreateStoryActions(story, cinematicModelPath);
        cameraActionQueue = CameraActionFactory.CreateCameraActions(story, cameraPlanPath);
        currentTime = 0;
        nextActionIndex = 0;
        //find total time for execution. not sure how to easily find this without searching a lot of actions
        //totalTime = yeah 
        //TODO move story load into el presidente

		float at = 0;
		KeyFrame current;
        foreach (IActorAction aa in actorActionQueue) 
		{
			if (aa.StartTick() > at + keyFrameFrequency)
			{

			}
		}

    }


    private void loadStructuredImpulsePlan(string storyPlanPath)
    {
        debugText.text = "beginning load " + storyPlanPath;
        Debug.Log("begin story plan xml load");
        var xml = Impulse.v_1_336.Xml.Story.LoadFromFile(storyPlanPath);
        Debug.Log("end story plan xml load");
        var factory = Impulse.v_1_336.StoryParsingFactories.GetUnsignedIntergerIntervalFactory();
        Debug.Log("begin story plan parse");
        story = factory.ParseStory(xml, false);//TODO true!
        Debug.Log("end story plan parse");
        debugText.text = "story load done!";
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

    public void setTime(float targetPercentComplete)
    {        
        //find previous keyframe from calculated time
        debugText.text = (targetPercentComplete * totalTime).ToString();
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
        while (nextActionIndex < actorActionQueue.Count && actorActionQueue[nextActionIndex].StartTick() <= currentTime)
        {
            IActorAction action = actorActionQueue[nextActionIndex];
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

    public float getCurrentTime()
    {
        return currentTime;
    }

	public void goTo(float time)
	{
        if (time < 0)
            time = 0;
        Debug.Log ("goto " + time);
		if (time < currentTime)
        {
            if (nextActionIndex >= actorActionQueue.Count)
                nextActionIndex--;
            while (nextActionIndex >= 0 && actorActionQueue[nextActionIndex].StartTick() > time)
            {
                actorActionQueue [nextActionIndex].Undo ();
                nextActionIndex--;
            }
            nextActionIndex++;
            lastTickLogged = time;
        }
        else
        {
            currentTime = time;
            List<IActorAction> removeList = new List<IActorAction>();
            foreach (IActorAction actorAction in executingActions)
            {
                if (actorActionComplete(actorAction))
                {
                    actorAction.Skip();
                    removeList.Add(actorAction);
                }
            }
            foreach (IActorAction action in removeList)
            {
                executingActions.Remove(action);
            }
            while (nextActionIndex < actorActionQueue.Count && actorActionQueue[nextActionIndex].EndTick() <= currentTime)
            {
                IActorAction action = actorActionQueue[nextActionIndex];
                nextActionIndex++;
                if (action.Init())
                {
                    action.Skip();
                }
            }
        }
		currentTime = time;
	}

    public void scaleTime(float scale)
    {
        Time.timeScale = scale;
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