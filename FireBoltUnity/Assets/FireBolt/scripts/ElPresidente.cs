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

        // Call the screenshot coroutine to create keyframe images for scrubbing.
        StartCoroutine(CreateScreenshots());

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
        Debug.Log ("rewind to " + actions.NextActionIndex + ": " + actions[actions.NextActionIndex]);
    }

    void fastForwardFireBoltActions(FireBoltActionList actions, float time, FireBoltActionList executingActions, float referenceTime)
    {
        List<IFireBoltAction> removeList = new List<IFireBoltAction>();
        foreach (IFireBoltAction action in executingActions)
        {
            if (actionComplete(action, referenceTime))
            {
                action.Skip();
                removeList.Add(action);
            }
        }
        foreach (IFireBoltAction action in removeList)
        {
            executingActions.Remove(action);
        }
        while (actions.NextActionIndex < actions.Count && actions[actions.NextActionIndex].StartTick() <= time) //TODO should probably encapsulate some more of this stuff in the list class
        {
            IFireBoltAction action = actions[actions.NextActionIndex];
            actions.NextActionIndex++;
            if (action.Init())
            {
                if (!actionComplete(action, referenceTime))
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

    /// <summary>
    /// Creates a series of screenshots to be used as keyframes for scrubbing.
    /// </summary>
    private IEnumerator CreateScreenshots ()
    {
        // Find the canvase game object.
        GameObject canvasGO = GameObject.Find("Canvas");

        // Get the canvas component from the game object.
        Canvas canvas = canvasGO.GetComponent<Canvas>();

        // Toggle the canvas display off.
        canvas.enabled = false;

        // Store the main camera's default settings.
        CameraClearFlags defClearFlags = Camera.main.clearFlags;
        Color defBackgroundColor = Camera.main.backgroundColor;
        int defCullingMask = Camera.main.cullingMask;

        // Make the main camera display a black screen while the system iterates through the keyframes.
        Camera.main.clearFlags = CameraClearFlags.SolidColor;
        Camera.main.backgroundColor = Color.black;
        Camera.main.cullingMask = 0;

        // Loop through discourse time at intervals of 20%.
        for (float i = 0; i < 100; i = i + 20)
        {
            // Set the time based on the current loop.
            setTime(i / 100);

            // Allow the frame to process.
            yield return new WaitForEndOfFrame();

            // Initialize the render texture and texture 2D.
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);

            // Create a new camera object and position it where the main camera is.
            GameObject testCameraGO = new GameObject();
            testCameraGO.transform.position = Camera.main.transform.position;
            testCameraGO.transform.rotation = Camera.main.transform.rotation;
            Camera test = testCameraGO.AddComponent<Camera>();

            // Render the texture.
            test.targetTexture = rt;
            test.Render();

            // Read the rendered texture into the texture 2D and reset the camera.
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            test.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            Destroy(testCameraGO);

            // Save the texture 2D as a PNG.
            byte[] bytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(@"Assets/screens/" + i + ".png", bytes);
        }

        // Reset the main camera to its default configuration.
        Camera.main.clearFlags = defClearFlags;
        Camera.main.backgroundColor = defBackgroundColor;
        Camera.main.cullingMask = defCullingMask;

        // Toggle the canvas display back on.
        canvas.enabled = true;

        // Reset the time to zero.
        setTime(0);
    }

    public void DisplayKeyframes()
    {
        Debug.Log("Hello World");
    }

    public void HideKeyframes()
    {
        Debug.Log("Goodbye World");
    }
}