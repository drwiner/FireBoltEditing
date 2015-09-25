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
using CM = CinematicModel;


public class ElPresidente : MonoBehaviour {

    FireBoltActionList actorActionList;
    DiscourseActionList discourseActionList;
    FireBoltActionList executingActorActions; 
    FireBoltActionList executingDiscourseActions;
    private float lastTickLogged;
    public Text debugText;
	public float myTime;
    public Slider whereWeAt;
    public static readonly ushort MILLIS_PER_FRAME = 5;
    private AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story;

    public static ElPresidente Instance;

    private AssetBundle actorsAndAnimations = null;
    private AssetBundle terrain = null;
    private bool initialized = false;
    private bool initNext = false;
    private bool initTriggered = false;

    private CM.CinematicModel cinematicModel = null;

    InputSet currentInputSet=null;
    DateTime storyPlanLastReadTimeStamp = DateTime.Now;
    bool reloadStoryPlan = false;
    
    DateTime cameraPlanLastReadTimeStamp = DateTime.Now;    
    bool reloadCameraPlan = false;
    
    DateTime cinematicModelPlanLastReadTimeStamp = DateTime.Now;
    bool reloadCinematicModel = false;

    DateTime actorsAndAnimationsBundleLastReadTimeStamp = DateTime.Now;
    bool reloadActorsAndAnimationsBundle = false;

    DateTime terrainBundleLastReadTimeStamp = DateTime.Now;
    bool reloadTerrainBundle = false;

    /// <summary>
    /// story time.  controlled by discourse actions
    /// </summary>
    public static float currentStoryTime;

    /// <summary>
    /// time as relates to playback scrubbing
    /// </summary>
    public static float currentDiscourseTime;

    public float CurrentStoryTime { get { return currentStoryTime; } }
    public float CurrentDiscourseTime { get { return currentDiscourseTime; } }

    void Start()
    {
        Instance = this;
    }

    /// <summary>
    /// wrapper for default args Init to use from UI button as default args methods are not visible in UI click event assignment in inspector
    /// </summary>
    /// <param name="a"></param>
    public void Init(float a)
    {
        Init(null, true);
    }

    //new class to hold specified input file paths.  

    public void Init(InputSet newInputSet, bool forceFullReload=false)
    {
        //if we didn't get handed one, generate an input set with the default paths
        if (newInputSet == null) 
        {
            newInputSet = new InputSet();
        }

        //don't have to do file modification timestamp compares.  just set all the load flags to reload everything
        if (forceFullReload || currentInputSet == null)
        {
            reloadStoryPlan = true;
            reloadCameraPlan = true;
            reloadCinematicModel = true;
            reloadActorsAndAnimationsBundle = true;
            reloadTerrainBundle = true;
        }
        else //we actually should figure out what's changed so we can reload only those required inputs
        {
            reloadStoryPlan = requiresReload(currentInputSet.StoryPlanPath, newInputSet.StoryPlanPath, storyPlanLastReadTimeStamp);
            reloadCameraPlan = requiresReload(currentInputSet.CameraPlanPath, newInputSet.CameraPlanPath, cameraPlanLastReadTimeStamp);
            reloadCinematicModel = requiresReload(currentInputSet.CinematicModelPath, newInputSet.CinematicModelPath, cinematicModelPlanLastReadTimeStamp);
            reloadActorsAndAnimationsBundle = requiresReload(currentInputSet.ActorsAndAnimationsBundlePath, newInputSet.ActorsAndAnimationsBundlePath, actorsAndAnimationsBundleLastReadTimeStamp);
            reloadTerrainBundle = requiresReload(currentInputSet.TerrainBundlePath, newInputSet.TerrainBundlePath, terrainBundleLastReadTimeStamp);
        }

        Destroy(GameObject.Find("InstantiatedObjects") as GameObject);
        if (reloadTerrainBundle) Destroy(GameObject.Find("Terrain") as GameObject);
        initialized = false;
        initTriggered = true;
        currentInputSet = newInputSet;
    }

    private bool requiresReload(string oldPath, string newPath, DateTime lastRead)
    {
        if(string.Compare(oldPath, newPath)==0 && //same file
           getFileLastModifiedTime(newPath) < lastRead)//file was last modified before the last time we read it.
        {
            return false;
        }
        return true;
    }

    private DateTime getFileLastModifiedTime(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogError(string.Format("file[{0}] cannot be found for FireBolt load. This will crash in a sec :)", path));            
        }
        return File.GetLastWriteTime(path);        
    }

    /// <summary>
    /// does the actual re-init work.  should only be called after destroy has had time to process.  
    /// currently this involves an elaborate set of bools to keep up with engine execution
    /// </summary>
    private void init()
    {
        if (reloadStoryPlan)
        {
            loadStructuredImpulsePlan(currentInputSet.StoryPlanPath);
            storyPlanLastReadTimeStamp = DateTime.Now;
        }

        if (reloadCinematicModel)
        {
            cinematicModel = CM.Parser.Parse(currentInputSet.CinematicModelPath);
            cinematicModelPlanLastReadTimeStamp = DateTime.Now;
        }

        if (actorsAndAnimations != null && reloadActorsAndAnimationsBundle)
            actorsAndAnimations.Unload(true);


        if (reloadActorsAndAnimationsBundle)
        {
            actorsAndAnimations = AssetBundle.CreateFromFile(currentInputSet.ActorsAndAnimationsBundlePath);
            actorsAndAnimationsBundleLastReadTimeStamp = DateTime.Now;
        }            

        if (terrain != null && reloadTerrainBundle)
            terrain.Unload(true);

        if (reloadTerrainBundle)
        {
            terrain = AssetBundle.CreateFromFile(currentInputSet.TerrainBundlePath);
            terrainBundleLastReadTimeStamp = DateTime.Now;
            instantiateTerrain();
        }  

        if (reloadStoryPlan || reloadActorsAndAnimationsBundle || reloadCinematicModel)
        {        
            actorActionList = ActorActionFactory.CreateStoryActions(story, cinematicModel);
        }

        if (reloadStoryPlan || reloadCameraPlan)
        {            
            discourseActionList = CameraActionFactory.CreateCameraActions(story, currentInputSet.CameraPlanPath);
            cameraPlanLastReadTimeStamp = DateTime.Now;
        }

        currentDiscourseTime = 0;
        currentStoryTime = 0;
        actorActionList.NextActionIndex = 0;
        discourseActionList.NextActionIndex = 0;

        executingActorActions = new FireBoltActionList(new ActionTypeComparer());
        executingDiscourseActions = new FireBoltActionList(new ActionTypeComparer());
        new GameObject("InstantiatedObjects").transform.SetParent((GameObject.Find("FireBolt") as GameObject).transform);

        initialized = true;
        initNext = false;
        initTriggered = false;

        reloadActorsAndAnimationsBundle = false;
        reloadCameraPlan = false;
        reloadCinematicModel = false;
        reloadStoryPlan = false;
        reloadTerrainBundle = false;
    }

    private void instantiateTerrain()
    {
        GameObject go = (terrain.LoadAsset(cinematicModel.Terrain.TerrainFileName) as GameObject);
        var t = Instantiate(go)as GameObject;
        t.name = "Terrain";
        Vector3 v;
        cinematicModel.Terrain.Location.TryParseVector3(out v);
        t.transform.position = v; 
        t.transform.SetParent(GameObject.Find("FireBolt").transform,true);
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

    public AssetBundle GetActiveAssetBundle()
    {
        if (actorsAndAnimations == null)
        {
            Debug.Log("attempting to load from asset bundle before it is set. " +
                      "use ElPresidente.SetActiveAssetBundle() to load an asset bundle");
            return null;
        }
        return actorsAndAnimations;
    }

    /// <summary>
    /// suspends/resumes execution 
    /// </summary>
    public void PauseToggle()
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
        if (discourseActionList == null)
            goToDiscourseTime(0);

        else if (Mathf.Abs(targetPercentComplete * discourseActionList.EndDiscourseTime - currentDiscourseTime) > MILLIS_PER_FRAME)
        {
            goToDiscourseTime(targetPercentComplete * discourseActionList.EndDiscourseTime);                
        }            
    }

    void Update()
    {
        if (!initialized && initNext)
            init();
        else if (!initialized)
            return;

        currentStoryTime += Time.deltaTime * 1000;
        currentDiscourseTime += Time.deltaTime * 1000;

        if (debugText != null)
            debugText.text = currentDiscourseTime.ToString() + " : " + currentStoryTime.ToString();
        if (whereWeAt && currentDiscourseTime < discourseActionList.EndDiscourseTime)
            whereWeAt.value = currentDiscourseTime / discourseActionList.EndDiscourseTime;
        myTime = currentStoryTime;
        logTicks();

        updateFireBoltActions(actorActionList, executingActorActions, currentStoryTime);
        updateFireBoltActions(discourseActionList, executingDiscourseActions, currentDiscourseTime);
    }

    void LateUpdate()
    {
        if (!initialized && initTriggered)
        {
            initNext = true;
            return;
        }
        else if (!initialized)
            return;

        foreach (IFireBoltAction action in executingDiscourseActions)
        {
            action.Execute();
        }

        foreach (IFireBoltAction action in executingActorActions)
        {
            action.Execute();
        }
    }

    void updateFireBoltActions(FireBoltActionList actions, FireBoltActionList executingActions, float referenceTime)
    {
        List<IFireBoltAction> removeList = new List<IFireBoltAction>();
        foreach (IFireBoltAction action in executingActions)
        {
            if (actionComplete(action,referenceTime) || action.StartTick() > referenceTime)
            {
                action.Stop();
                removeList.Add(action);
            }
        }
        foreach (IFireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= referenceTime) //TODO should probably encapsulate some more of this stuff in the list class
        {
            IFireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init())
			{
                if (actionComplete(action, referenceTime))
                    action.Skip();                
                else
                    executingActions.Add(action);                     
			}
        }
    }

    void rewindFireBoltActions(FireBoltActionList actions, float time)
    {
        int currentIndex = actions.NextActionIndex-1;//next action was pointed to...next action!
        actions.NextActionIndex = 0;
        while (actions.NextActionIndex < actions.Count &&
               actions[actions.NextActionIndex].EndTick() < time) 
        {
            actions.NextActionIndex++;
        }

        while (currentIndex >= actions.NextActionIndex)
        {
            actions[currentIndex].Undo();
            currentIndex--;
        }
        if (actions.Count > actions.NextActionIndex)
            Debug.Log("rewind to " + actions.NextActionIndex + ": " + actions[actions.NextActionIndex]);
        else
            Debug.Log("rewind to action #" + actions.NextActionIndex);
    }

    void fastForwardFireBoltActions(FireBoltActionList actions, float targetTime, FireBoltActionList executingActions, float currentTime)
    {
        List<IFireBoltAction> removeList = new List<IFireBoltAction>();
        foreach (IFireBoltAction action in executingActions)
        {
            if (actionComplete(action, currentTime))
            {
                action.Skip();
                removeList.Add(action);
            }
        }
        foreach (IFireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= targetTime) //TODO should probably encapsulate some more of this stuff in the list class
        {
            IFireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init())
            {
                if (!actionComplete(action, currentTime))
                    executingActions.Add(action);
                else
                    action.Skip();
            }
        }
    }

	public void goToStoryTime(float time)
	{
        if (time < 0)
            time = 0;
        Debug.Log ("goto story " + time);
		if (time < currentStoryTime)
        {
            currentStoryTime = time;
            rewindFireBoltActions(actorActionList, currentStoryTime);
        }
        else
        {
            currentStoryTime = time;
            fastForwardFireBoltActions(actorActionList, currentStoryTime, executingActorActions, currentStoryTime);
        }
		currentStoryTime = time;
	}

    public void goToDiscourseTime(float time)
    {
        if (time < 0)
            time = 0;
        Debug.Log("goto discourse " + time);
        lastTickLogged = time;
        if (time < currentDiscourseTime)
        {
            currentDiscourseTime = time;
            rewindFireBoltActions(discourseActionList, currentDiscourseTime);
        }
        else
        {
            currentDiscourseTime = time;
            fastForwardFireBoltActions(discourseActionList, currentDiscourseTime, executingDiscourseActions, currentDiscourseTime);
        }
        currentDiscourseTime = time;
    }

    public void scaleTime(float scale)
    {
        Time.timeScale = scale;
    }

	public void goToRel(float time)
	{
		goToStoryTime(currentStoryTime + time);
	}

    void logTicks()
    {
        if (currentDiscourseTime - lastTickLogged > 1000)
        {
            Debug.Log(currentDiscourseTime + " : " + currentStoryTime);
            lastTickLogged = currentDiscourseTime;
        }
    }

    bool actionComplete(IFireBoltAction action, float  referenceTime)
    {
        return action.EndTick() < referenceTime;
    }
}