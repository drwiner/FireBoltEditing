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
    FireBoltActionList executingActions;
    public string storyPlanPath;
    public string cinematicModelPath;
    public string cameraPlanPath;
    private float lastTickLogged;
    private float totalTime;
    public Text debugText;
	public float myTime;
    public Slider whereWeAt;
    public static readonly ushort MILLIS_PER_FRAME = 5;
    private AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story;

    public static ElPresidente Instance;

    private AssetBundle assetBundle = null;
    private const string ASSET_BUNDLE_DEFAULT = "AssetBundles/default";


    /// <summary>
    /// FireBolt point of truth for time.  updated with but independent of time.deltaTime
    /// expressed in milliseconds
    /// </summary>
    public static float currentTime;

	// Use this for initialization
	void Start () {        
        Init(storyPlanPath,cameraPlanPath,cinematicModelPath);
    }

/// <summary>
/// 
/// </summary>
/// <param name="newStoryPlanPath"></param>
/// <param name="newCameraPlanPath"></param>
/// <param name="newCinematicModelPath"></param>
/// <param name="newAssetBundlePath"></param>
    public void Init(string newStoryPlanPath, string newCameraPlanPath, string newCinematicModelPath, string newAssetBundlePath=ASSET_BUNDLE_DEFAULT)
    {
        this.storyPlanPath = newStoryPlanPath;
        this.cameraPlanPath = newCameraPlanPath;
        this.cinematicModelPath = newCinematicModelPath;

        executingActions = new FireBoltActionList(new ActionTypeComparer());
        loadStructuredImpulsePlan(storyPlanPath);
        actorActionList = ActorActionFactory.CreateStoryActions(story, cinematicModelPath);
        cameraActionList = CameraActionFactory.CreateCameraActions(story, cameraPlanPath);
        currentTime = 0;
        //find total time for execution. not sure how to easily find this without searching a lot of actions
        totalTime = 0;
        if (actorActionList.Count > 0)
            totalTime = actorActionList[actorActionList.Count - 1].EndTick() - actorActionList[0].StartTick();

        Instance = this;
        SetActiveAssetBundle(newAssetBundlePath); 
    }

    private void loadStructuredImpulsePlan(string storyPlanPath)
    {
        Debug.Log("begin story plan xml load");
        var xml = Impulse.v_1_336.Xml.Story.LoadFromFile(storyPlanPath);
        Debug.Log("end story plan xml load");
        var factory = Impulse.v_1_336.StoryParsingFactories.GetUnsignedIntergerIntervalFactory();
        Debug.Log("begin story plan parse");
        story = factory.ParseStory(xml, false);//TODO true! get crackin with that validation, colin!
        Debug.Log("end story plan parse");
    }

    /// <summary>
    /// provide an asset bundle path on local disk for el presidente to load from
    /// </summary>
    /// <param name="bundlePath">may be absolute or relative to execution directory</param>
    public void SetActiveAssetBundle(string bundlePath)
    {
        assetBundle = AssetBundle.CreateFromFile(bundlePath);
    }


    public AssetBundle GetActiveAssetBundle()
    {
        if (assetBundle == null)
        {
            Debug.Log("attempting to load from asset bundle before it is set. " +
                      "use ElPresidente.SetActiveAssetBundle() to load an asset bundle");
            return null;
        }
        return assetBundle;
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
        if (Mathf.Abs(targetPercentComplete * totalTime - currentTime) > MILLIS_PER_FRAME)
            goTo (targetPercentComplete * totalTime);
    }

    void Update()
    {
        currentTime += Time.deltaTime * 1000;
        if(debugText != null)
            debugText.text = currentTime.ToString();
        if (whereWeAt && currentTime < totalTime)
            whereWeAt.value = currentTime / totalTime;
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
                if (actorActionComplete(action))
                    action.Skip();                
                else
                    executingActions.Add(action);                     
			}
        }
    }

    void rewindFireBoltActions(FireBoltActionList actions)
    {
        int currentIndex = actions.NextActionIndex-1;//next action was pointed to...next action!
        actions.NextActionIndex = 0;
        while (actions.NextActionIndex < actions.Count &&
               actions[actions.NextActionIndex].EndTick() < currentTime)
        {
            actions.NextActionIndex++;
        }

        while (currentIndex >= actions.NextActionIndex)
        {
            actions[currentIndex].Undo();
            currentIndex--;
        }
        Debug.Log ("rewind to " + actions.NextActionIndex + ": " + actions[actions.NextActionIndex]);
    }

    void fastForwardFireBoltActions(FireBoltActionList actions)
    {
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
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= currentTime) //TODO should probably encapsulate some more of this stuff in the list class
        {
            IFireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init())
            {
                if (!actorActionComplete(action))
                    executingActions.Add(action);
                else
                    action.Skip();
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
        lastTickLogged = time;
		if (time < currentTime)
        {
            currentTime = time;
            rewindFireBoltActions(actorActionList);
            rewindFireBoltActions(cameraActionList);
        }
        else
        {
            currentTime = time;
            fastForwardFireBoltActions(actorActionList);
            fastForwardFireBoltActions(cameraActionList);
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