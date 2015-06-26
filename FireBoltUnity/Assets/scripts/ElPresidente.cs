using UnityEngine;
using UnityEngine.UI;
using System.Xml;
using System.IO;
using System.Collections;
using Assets.scripts;
using System.Collections.Generic;
using System;
using Impulse.v_1_336;
using UintT = Impulse.v_1_336.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;


public class ElPresidente : MonoBehaviour {

    FireBoltActionList actorActionList;
    FireBoltActionList cameraActionList;
    List<IFireBoltAction> executingActions;
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
    public static readonly ushort MILLIS_PER_FRAME = 5;
    private AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story;


    /// <summary>
    /// FireBolt point of truth for time.  updated with but independent of time.deltaTime
    /// expressed in milliseconds
    /// </summary>
    public static float currentTime;

	// Use this for initialization
	void Awake () {
        ActorActionFactory.debugText = debugText;
        executingActions = new List<IFireBoltAction>();
        keyFrames = new List<Keyframe>();
        loadStructuredImpulsePlan(storyPlanPath);
        actorActionList = ActorActionFactory.CreateStoryActions(story, cinematicModelPath);
        cameraActionList = CameraActionFactory.CreateCameraActions(story, cameraPlanPath);
        currentTime = 0;        
        //find total time for execution. not sure how to easily find this without searching a lot of actions
        //totalTime = yeah 
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

        updateFireBoltActions(actorActionList);
        updateFireBoltActions(cameraActionList);
    }

    void updateFireBoltActions(FireBoltActionList actions)
    {
        List<IFireBoltAction> removeList = new List<IFireBoltAction>();
        foreach (IFireBoltAction actorAction in executingActions)
        {
            if (actorActionComplete(actorAction) || actorAction.StartTick() > currentTime)
            {
                actorAction.Stop();
                removeList.Add(actorAction);
            }
        }
        foreach (IFireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= currentTime) //TODO should probably encapsulate some more of this stuff in the list class
        {
            IFireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
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
        foreach (IFireBoltAction actorAction in executingActions)
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
            if (nextActionIndex >= actorActionList.Count)
                nextActionIndex--;
            while (nextActionIndex >= 0 && actorActionList[nextActionIndex].StartTick() > time)
            {
                actorActionList [nextActionIndex].Undo ();
                nextActionIndex--;
            }
            nextActionIndex++;
            lastTickLogged = time;
        }
        else
        {
            currentTime = time;
            List<IFireBoltAction> removeList = new List<IFireBoltAction>();
            foreach (IFireBoltAction actorAction in executingActions)
            {
                if (actorActionComplete(actorAction))
                {
                    actorAction.Skip();
                    removeList.Add(actorAction);
                }
            }
            foreach (IFireBoltAction action in removeList)
            {
                executingActions.Remove(action);
            }
            while (actorActionList.NextActionIndex < actorActionList.Count && actorActionList[nextActionIndex].EndTick() <= currentTime)
            {
                IFireBoltAction action = actorActionList[nextActionIndex];
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

    bool actorActionComplete(IFireBoltAction iaa)
    {
        return iaa.EndTick() < currentTime;
    }
}