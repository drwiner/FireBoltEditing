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
    FireBoltActionList cameraActionList;
    FireBoltActionList executingActions;
    private float lastTickLogged;
    private float totalTime;
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


    //TODO add overrides for Init to specify individual paths or at least the story and camera plans since those will change most often.
    //then only reload what you need when you need it
    //still have to deal with re-init before destroy completion

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
    /// FireBolt point of truth for time.  updated with but independent of time.deltaTime
    /// expressed in milliseconds
    /// </summary>
    public static float currentTime;

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
        executingActions = new FireBoltActionList(new ActionTypeComparer());
        initialized = false;
        initTriggered = true;
    }

    /// <summary>
    /// does the actual re-init work.  should only be called after destroy has had time to process.  
    /// currently this involves an elaborate set of bools to keep up with engine execution
    /// </summary>
    private void init()
    {
        setTime(0);
        executingActions = new FireBoltActionList(new ActionTypeComparer());
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
            totalTime = 0;
            //find total time for execution. not sure how to easily find this without searching a lot of actions
            //current solution is not always correct
            if (actorActionList.Count > 0)
                totalTime = actorActionList[actorActionList.Count - 1].EndTick() - actorActionList[0].StartTick();
        }

        if(reloadStoryPlan || reloadCameraPlan)
            cameraActionList = CameraActionFactory.CreateCameraActions(story, cameraPlanPath);

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
        if (Mathf.Abs(targetPercentComplete * totalTime - currentTime) > MILLIS_PER_FRAME)
            goTo (targetPercentComplete * totalTime);
    }

    void Update()
    {
        if (!initialized && initNext)
            init();
        else if (!initialized)
            return;
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

    void LateUpdate()
    {
        if (!initialized && initTriggered)
        {
            initNext = true;
            return;
        }
        else if (!initialized)
            return;

        foreach (IFireBoltAction actorAction in executingActions)
        {
            actorAction.Execute();
        }
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