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

    string storyPlanPath;
    bool reloadStoryPlan = false;
    string cameraPlanPath;
    bool reloadCameraPlan = false;
    string cinematicModelPath;
    bool reloadCinematicModel = false;
    string actorsAndAnimationsBundlePath;
    bool reloadActorsAndAnimationsBundle = false;
    string terrainBundlePath;
    bool reloadTerrainBundle = false;

    void setStoryPlanPath(string path)
    {
        if (string.Compare(storyPlanPath, path) != 0)
            reloadStoryPlan = true;
        this.storyPlanPath = path;
    }

    void setCameraPlanPath(string path)
    {
        if (string.Compare(cameraPlanPath, path) != 0)
            reloadCameraPlan = true;
        this.cameraPlanPath = path;
    }

    void setCinematicModelPath(string path)
    {
        if (string.Compare(cinematicModelPath, path) != 0)
            reloadCinematicModel = true;
        this.cinematicModelPath= path;
    }

    void setActorsAndAnimationsBundlePath(string path)
    {
        if (string.Compare(actorsAndAnimationsBundlePath, path) != 0)
            reloadActorsAndAnimationsBundle = true;
        this.actorsAndAnimationsBundlePath = path;
    }

    void setTerrainBundlePath(string path)
    {
        if (string.Compare(terrainBundlePath, path) != 0)
            reloadTerrainBundle = true;
        this.terrainBundlePath = path;
    }

    /// <summary>
    /// story time.  controlled by discourse actions
    /// </summary>
    public static float currentStoryTime;

    /// <summary>
    /// time as relates to playback scrubbing
    /// </summary>
    public static float currentDiscourseTime;

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
        Init();
    }

    /// <summary>
    /// set paths for assets to use in firebolt and restart system with freshly loaded assets on next update.  all arguments optional.  
    /// </summary>
    /// <param name="storyPlanPath"></param>
    /// <param name="cameraPlanPath"></param>
    /// <param name="cinematicModelPath"></param>
    /// <param name="actorsAndAnimationsBundlePath"></param>
    /// <param name="terrainBundlePath">no terrain loading currently</param>
    public void Init(string storyPlanPath = "storyPlans/defaultStory.xml", string cameraPlanPath = "cameraPlans/defaultCamera.xml", 
                     string cinematicModelPath = "cinematicModels/defaultModel.xml", string actorsAndAnimationsBundlePath = "AssetBundles/actorsandanimations", 
                     string terrainBundlePath = "AssetBundles/terrain")
    {
        setStoryPlanPath(storyPlanPath);
        setCameraPlanPath(cameraPlanPath);
        setCinematicModelPath(cinematicModelPath);
        setActorsAndAnimationsBundlePath(actorsAndAnimationsBundlePath);
        setTerrainBundlePath(terrainBundlePath);


        Destroy(GameObject.Find("InstantiatedObjects") as GameObject);
        if (reloadTerrainBundle) Destroy(GameObject.Find("Terrain") as GameObject);
        initialized = false;
        initTriggered = true;
    }

    /// <summary>
    /// does the actual re-init work.  should only be called after destroy has had time to process.  
    /// currently this involves an elaborate set of bools to keep up with engine execution
    /// </summary>
    private void init()
    {
        currentDiscourseTime = 0;
        currentStoryTime = 0;
        executingActorActions = new FireBoltActionList(new ActionTypeComparer());
        executingDiscourseActions = new FireBoltActionList(new ActionTypeComparer());
        new GameObject("InstantiatedObjects").transform.SetParent((GameObject.Find("FireBolt") as GameObject).transform);

        if (reloadStoryPlan)
            loadStructuredImpulsePlan(storyPlanPath);

        if(reloadCinematicModel)
            cinematicModel = CM.Parser.Parse(cinematicModelPath);

        if(actorsAndAnimations!=null && reloadActorsAndAnimationsBundle)
            actorsAndAnimations.Unload(true);            

        if (reloadActorsAndAnimationsBundle)
            actorsAndAnimations = AssetBundle.CreateFromFile(actorsAndAnimationsBundlePath);

        if (terrain != null && reloadTerrainBundle)
            terrain.Unload(true);

        if (reloadTerrainBundle)
        {
            terrain = AssetBundle.CreateFromFile(terrainBundlePath);
            loadTerrain();
        }  

        if (reloadStoryPlan || reloadActorsAndAnimationsBundle || reloadCinematicModel)
        {        
            actorActionList = ActorActionFactory.CreateStoryActions(story, cinematicModel);
        }

        if (reloadStoryPlan || reloadCameraPlan)
        {            
            discourseActionList = CameraActionFactory.CreateCameraActions(story, cameraPlanPath);
        }

        initialized = true;
        initNext = false;
        initTriggered = false;

        reloadActorsAndAnimationsBundle = false;
        reloadCameraPlan = false;
        reloadCinematicModel = false;
        reloadStoryPlan = false;
        reloadTerrainBundle = false;
    }

    private void loadTerrain()
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
        if(debugText != null)
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
               actions[actions.NextActionIndex].EndTick() < time) //this may orphan actions in the executing list until they get replayed and hit their stop time again.  seems like a leak, but a bounded one
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



    public float CurrentStoryTime { get { return currentStoryTime; } }
    public float CurrentDiscourseTime { get { return currentDiscourseTime; } }

	public void goToStoryTime(float time)
	{
        if (time < 0)
            time = 0;
        Debug.Log ("goto story " + time);
		if (time < currentStoryTime)
        {
            currentStoryTime = time;
            rewindFireBoltActions(actorActionList, currentStoryTime);
            //rewindFireBoltActions(cameraActionList);
        }
        else
        {
            currentStoryTime = time;
            fastForwardFireBoltActions(actorActionList, currentStoryTime, executingActorActions, currentStoryTime);
            //fastForwardFireBoltActions(cameraActionList);
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
            //rewindFireBoltActions(actorActionList);
            rewindFireBoltActions(discourseActionList, currentDiscourseTime);
        }
        else
        {
            currentDiscourseTime = time;
            //fastForwardFireBoltActions(actorActionList);
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