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
        aaq = ActorActionFactory.CreateStoryActions(story, cinematicModelPath);
        currentTime = 0;
        nextActionIndex = 0;
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
        logTicks();
        List<IActorAction> removeList = new List<IActorAction>();
        foreach (IActorAction actorAction in executingActions)
        {
            if (actorActionComplete(actorAction))
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