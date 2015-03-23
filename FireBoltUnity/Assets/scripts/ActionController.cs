using UnityEngine;
using Impulse.v_0_1;
using ImpulsePlan = Impulse.v_0_1.Plan;
using System.Xml;
using System.IO;
using System.Collections;
using Bolt.Legacy;
using Assets.scripts;

public class ActionController : MonoBehaviour {

    StoryPlan storyPlan;

	// Use this for initialization
	void Start () {
	
	}

	
	// Update is called once per frame
	void Update () {
	
	}

    /// <summary>
    /// interacts with implulse to load the given story plan file
    /// </summary>
    /// <param name="planFileName"></param>
    private void LoadStoryPlan(string planFileName){

    }

}


namespace Bolt {

	/// <summary>
	/// The ActionController is the core of the Bolt framework. Make sure that exactly 
	/// one copy Component is attached to a GameObject in your scene. If you would like
	/// the ability to scrub in your scene, use the <see cref="ScrubbingActionController"/>
	/// class instead.
	/// </summary>
	public abstract class ActionController : MonoBehaviour  {
	
		// Name of the temp directory where Bolt stores keyframes, etc.
        protected const string PLANCACHE = "plancache";

		// This is a singleton class with a single instance
        private static ActionController instance;

		// The story and camera plan can be provided as Unity text assets
		public TextAsset storyPlanAsset, cameraPlanAsset;
		// Since Bolt uses the Impulse loader for story plans, you must provide a name.
		// This allows the loader to cross-reference with other loaded plans
		public string storyPlanName;
		// Indicates whether the movie should loop when finished
		public bool repeat;

		protected StoryPlan storyPlan;
		protected CameraPlan cameraPlan;
		// A list of currently executing story actions, for which the Update() method will be called
		protected List<PlanAction> executingActions = new List<PlanAction>();
		// A lookup to get actors by their name in the story plan
		protected Dictionary<string, GameObject> actors = new Dictionary<string, GameObject>();
		// If a Serializer Component is added to our GameObject, we will use it to cache Keyframes
		protected Serializer serializer;
		// Hash of the text content of the story and camera plans
        protected int cinematicHash;
		// Indicates if caching should be used (specifically for Keyframes).
		// While changes to the plans should cause these files to regenerate,
		// it can be helpful sometimes to disable caching to ensure a change
		// has taken effect.
        protected bool useCache = true;

		// The tick (in story-time) of the last Update
		private float lastTick;
		// If false, the controller will stop executing and hist any associated components
		// Note that this is different from pausing.
		private bool running;

		/// <summary>
		/// Should create a StoryPlan specific to domain of the story
		/// </summary>
		/// <returns>The story plan.</returns>
		protected abstract StoryPlan CreateStoryPlan();
		/// <summary>
		/// Should convery story-time ticks into seconds
		/// </summary>
		/// <returns>The time in seconds.</returns>
		/// <param name="time">The time in ticks</param>
		protected abstract float TimeToTickF(float time);
		/// <summary>
		/// This method is a reminder to register any story actions
		/// you have created with the <see cref="PlanAction"/> class
		/// so that they can be instantiated properly.
		/// </summary>
		protected abstract void RegisterActions();

        public float LastTick { get { return lastTick; } }
        public float LastTickSeconds { get { return lastTick / TimeToTickF(1); } }
        
        public IEnumerable<PlanAction> ExecutingActions { get { return executingActions; } }

		/// <summary>
		/// Gets the story plan, containing all actions that are executed
		/// in the game, regardless of their context in the discourse.
		/// </summary>
		/// <value>The story plan.</value>
		public StoryPlan StoryPlan { 
			get { return storyPlan; } 
		}
		
		/// <summary>
		/// Gets the camera plan with methods for adjusting movie time in terms
		/// of progress [0-1] through the movie.
		/// </summary>
		/// <value>The camera plan.</value>
		public CameraPlan CameraPlan {
			get { return cameraPlan; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether this <see cref="Bolt.ActionController"/> is running.
		/// </summary>
		/// <value><c>true</c> if running; otherwise, <c>false</c>.</value>
		public bool Running { 
			get { return running; } 
			set { if (value == running) return; if (value) StartPlayback(); else StopPlayback(); }
		}

		/// <summary>
		/// Returns the singleton instance of the ActionController
		/// </summary>
		/// <value>The instance.</value>
        public static ActionController Instance {
            get { return instance; }
        }

		/// <summary>
		/// Returns the camera tagged MainCamera
		/// </summary>
		/// <value>The main camera.</value>
        public static Camera MainCamera {
            get { return GameObject.FindWithTag("MainCamera").camera; }
        }
        
		protected virtual void Awake() {
			instance = this;
        }
		
		protected virtual void Start () {
			instance = this;
			RegisterActions();
			serializer = GetComponent<Serializer>();

			// By default we try to load plans provided as assets
			// in the Unity editor. If this fails, they can be loaded
			// manually with the LoadPlan method.
            LoadPlansFromAssets();
		}

		/// <summary>
		/// Attempts to load the plans provided as text assets in the Unity editor
		/// </summary>
        protected void LoadPlansFromAssets() {
            if (storyPlanAsset != null && cameraPlanAsset != null) {
                string storyPlanXml = storyPlanAsset.text;
                string cameraPlanXml = cameraPlanAsset.text;
				// We hash the content of the plans so we know whether we can load content from cache
                cinematicHash = Math.Abs(storyPlanXml.GetHashCode() * 31 + cameraPlanXml.GetHashCode());
                LoadPlan(storyPlanXml, cameraPlanXml);
                StartPlayback();
            }
		}
		
		/// <summary>
		/// Activates the player and starts playing the loaded plan. You must call LoadPlan()
		/// before calling this method.
		/// </summary>
		public virtual void StartPlayback() {
			if (storyPlan == null || cameraPlan == null) {
				throw new Exception("No camera and story plan loaded. Use LoadPlan() first.");
			}		
			running = true;
		}
		
		/// <summary>
		/// Stops playback of the plan and hides the associated UI.
		/// </summary>
		public virtual void StopPlayback() {
			running = false;
		}
		
		/// <summary>
		/// Resets the playback of the plan to the beginning. 
		/// </summary>
		public virtual void ResetPlayback() {
			ResetPlans();
		}
		
		/// <summary>
		/// Loads the story and camera plans from XML. This call can take a while for
		/// longer plans. You must then call StartPlayback to play the loaded plans.
		/// </summary>
		/// <param name="storyPlanXml">Story plan xml.</param>
		/// <param name="cameraPlanXml">Camera plan xml.</param>
		public void LoadPlan(string storyPlanXml, string cameraPlanXml) {
			ClearPlans();
		
			storyPlan = CreateStoryPlan(storyPlanXml);
			cameraPlan = CreateCameraPlan(cameraPlanXml, storyPlan);
		}
		
		/// <summary>
		/// The the Xform for the given actor at the given tick, as 
		/// determined by the story plan (and occasionally the discourse).
		/// Note that the *current* CameraAction will be used to reposition
		/// actors, which will not necessarily be active at the tick provided.
		/// </summary>
		/// <returns>The actor xform.</returns>
		/// <param name="actor">Actor.</param>
		/// <param name="tick">Tick.</param>
		/// <param name="ignoreDiscourse">Set to true to ignore the CameraPlan's repositionings </param> 
		public Xform GetActorXform(GameObject actor, float tick, bool ignoreDiscourse = false) {
			return GetActorXform(actor.name, tick, ignoreDiscourse);
		}
		
		public Xform GetActorXform(string actor, float tick, bool ignoreDiscourse = false) {
		
			if (!ignoreDiscourse) {
				// If our current CameraAction wants to move an actor for discourse purposes
				// then we use that formost
				CameraAction cameraAction = cameraPlan.GetCurrentCameraAction();
				foreach (ActorReposition rep in cameraAction.GetRepositions(tick)) {
					if (rep.actor == actor) {
						return rep.xform;
					}
				}
			}
					
			// Otherwise get the most recent MoveAction for this actor
			IMoveAction action = (IMoveAction) storyPlan.GetLastAction((int)tick, (PlanAction a) =>  { 
				return (a is IMoveAction && ((IMoveAction) a).GetActorName() == actor); 
			});
			
			// Or finally just give up and return an empty camera
			if (action == null) {
				return Xform.CreateDefault();
			}
			
			Xform xform = action.GetActorXform(tick, ignoreDiscourse);
			
			return xform;
		}

		/// <summary>
		/// Creates a StoryPlan from the given xml. This process uses
		/// Impulse to parse the xml and then reads the relevent parameters.
		/// </summary>
		/// <returns>The story plan.</returns>
		/// <param name="storyPlanXml">Story plan xml.</param>
		protected StoryPlan CreateStoryPlan(string storyPlanXml) {
			if (!ImpulsePlan.IsLoaded(storyPlanName)) {
				ImpulsePlan.LoadFromString(storyPlanXml);
			}
			
			ImpulsePlan plan = ImpulsePlan.GetPlan(storyPlanName);
			StoryPlan storyPlan = CreateStoryPlan();
			foreach (Step step in plan.Steps) {
				// we can only load actions that have been registered
				if (!PlanAction.ActionExists(step.Name)) continue;
				PlanAction action = PlanAction.Create(step.Name);
				Dictionary<string, string> ps = new Dictionary<string, string>();
				foreach (Parameter param in step.Parameters) {
					ps.Add(param.Name, param.Value);
				}
				// even though id is not a parameter, it's still
				// easier to pass it this way
				ps.Add("id", step.ID.ToString());
				// the Action needs a reference to this controller
				// before the parameters can be loaded
				action.SetController(this);
				action.LoadParameters(ps);
				storyPlan.AddAction(action);
			}

			// If the plan is partially ordered, totally order it
			storyPlan.CalculateIntervals();
			// Sort actions by start time
			storyPlan.Sort();
			return storyPlan;
		}

		/// <summary>
		/// Creates a CameraPlan with the given xml, referencing the given
		/// story plan.
		/// </summary>
		/// <returns>The camera plan.</returns>
		/// <param name="cameraPlanXml">Camera plan xml.</param>
		/// <param name="storyPlan">Story plan.</param>
		protected CameraPlan CreateCameraPlan(string cameraPlanXml, StoryPlan storyPlan) {
			XmlDocument xd = new XmlDocument();
			xd.LoadXml(cameraPlanXml);

			// Not that camera plan xml is a superset of the Impulse spec
			// and therefore must be parsed manually
			CameraPlan cameraPlan = new CameraPlan();
			foreach (XmlElement e in xd.GetElementsByTagName("step")) {
				CameraAction action = CreateCameraAction(e);
				if (action != null) cameraPlan.AddAction(action);
			}
			// Calculate intervals for actions that reference actions
			// rather than giving exact intervals
			cameraPlan.CalculateIntervals(storyPlan);
			return cameraPlan;
		}
		
		/// <summary>
		/// Called when the playback is reset. Override with any domain-specific
		/// logic that should be called when the plan resets.
		/// </summary>
		protected virtual void ResetPlans() {
			foreach (GameObject actor in actors.Values) {
				actor.SetActive(false);
			}
			executingActions.Clear();
			lastTick = 0;
			
			storyPlan.ResetPlan();
			cameraPlan.ResetPlan();
		}
		
		/// <summary>
		/// Called when the current plans are being cleared from memory. Override
		/// with any domain-specific logic so a newly loaded plan will be interfered
		/// with by residual data from the previous plan.
		/// </summary>
		protected virtual void ClearPlans() {
			foreach (GameObject actor in actors.Values) {
				GameObject.Destroy(actor);
			}
			executingActions.Clear();
			lastTick = 0;
			
			actors.Clear();
			storyPlan = null;
			cameraPlan = null;
		}

		private CameraAction CreateCameraAction(XmlElement e) {
			// CameraActions are PlanActions, so we use that as a base
			CameraAction action = (CameraAction) CreateAction(e);

			// PhaseCameraActions are the pre-Bolt way of expressing things,
			// but we still support them
			if (action is PhaseCameraAction) {
				foreach (XmlElement node in e.GetDirectElementsInList("phase")) {
					// They have subsections called Phases that must be parsed as well
					PhaseCameraAction.Phase phase = new PhaseCameraAction.Phase(node);
					((PhaseCameraAction) action).AddPhase(phase);
				}
			// The modern camera actions are called IdiomActions
			} else if (action is IdiomAction) {
				// These have subsections called Shots, which must be instanitated
				foreach (XmlElement node in e.GetDirectElementsInList("shot")) {
					CameraShot shot = Instantiator.Instantiate(node, (string name, Dictionary<string,string> parameters) => {
						return CameraShot.Create(name, parameters, this);
					}, "start");
					((IdiomAction) action).AddShot(shot);
				}
				// The same is true for Transitions
				foreach (XmlElement node in e.GetDirectElementsInList("transition")) {
					Transition trans = Instantiator.Instantiate(node, new Constructor<Transition>(Transition.Create));
					((IdiomAction) action).AddTransition(trans);
					// ...which themselves have Conditions
					foreach (XmlElement cNode in node.GetDirectElementsInList("condition")) {
						Condition cond = Instantiator.Instantiate(cNode, new Constructor<Condition>(Condition.Create));
						((IdiomAction) action).AddCondition(trans, cond);	
					}
				}
				((IdiomAction) action).Init();
			}
			return action;
		}

		// Holds the information needed to instantiate a PlanAction
		// Not particularly necessary - sort of a holdover from old code
		[Serializable]
		private struct ParameterizedInfo {
			public string actionType;
			public Dictionary<string, string> parameters;
		}

		private PlanAction CreateAction(XmlElement e) {
			ParameterizedInfo? info = CreateActionInfo(e);
			if (info == null) return null;
			return CreateAction((ParameterizedInfo) info);
		}
		
		private PlanAction CreateAction(ParameterizedInfo info) {
			PlanAction action = PlanAction.Create(info.actionType);
			if (action != null) {
				action.SetController(this);
				action.LoadParameters(info.parameters);
			}
			return action;
		}

		// The method essentially does the work the Impulse now does,
		// but because CameraActions are a superspec, we have to load
		// them manually.
		// TODO: See if it's possible to load this with Impulse and then
		// add the extra bits manually
		private static ParameterizedInfo? CreateActionInfo(XmlElement e) {
			XmlNodeList nameTags = e.GetElementsByTagName("name");
			if (nameTags.Count > 0) {
				string name = nameTags[0].InnerText;
				if (!PlanAction.ActionExists(name)) return null;
				string id = e.GetAttribute("id");
				ParameterizedInfo action = new ParameterizedInfo();
				action.actionType = name;
				Dictionary<string, string> parameters = new Dictionary<string, string>();
				parameters.Add("id", id);
				foreach (XmlElement param in e.GetDirectElementsInList("parameter")) {
					string paramName = param.GetAttribute("name").ToLower();
					if (!parameters.ContainsKey(paramName)) {
						parameters.Add(paramName, param.InnerText);
					}
				}
				action.parameters = parameters;
				return action;
			}
			return null;
		}

		/// <summary>
		/// Should update the movie time for one update
		/// </summary>
		protected virtual void UpdateCameraTime() {
			cameraPlan.Advance(TimeToTickF(Time.deltaTime), repeat);
		}
		
		protected virtual void Update () {
			if (!running) return;
			// Update the movie time, then update our executing actions
			UpdateCameraTime();
			float tick = cameraPlan.Tick;
			ExecuteActions(tick);
		}

		protected virtual void LateUpdate() {
			if (!running) return;
			UpdateCameraPlan();
		}
		
		protected virtual void UpdateCameraPlan() {
			cameraPlan.Update();
		}

		/// <summary>
		/// Should update all actions for this frame
		/// </summary>
		/// <param name="tick">Tick.</param>
		protected virtual void ExecuteActions(float tick) {
			ExecuteActions(tick, storyPlan.GetNewActions((int) tick));
		}

		/// <summary>
		/// Executes the executingActions, adding the given actions and
		/// starting them, as well as removing and ending finished actions.
		/// </summary>
		/// <param name="tick">Tick.</param>
		/// <param name="actions">Actions.</param>
		protected void ExecuteActions(float tick, List<PlanAction> actions) {
			lastTick = cameraPlan.Tick;
			executingActions.AddRange(actions);
			foreach (PlanAction action in actions) {
				action.Start(tick);
			}
			foreach (PlanAction action in executingActions) {
				action.Update(tick);
				if (action is IMoveAction) {
					UpdateMovement((IMoveAction)action);
				}
			}
			for (int i = 0; i < executingActions.Count; i++) {
				if (executingActions[i].IsFinished((int)tick)) {
					executingActions[i].End(tick);
					executingActions.RemoveAt(i--);
				}
			}
		}
		
		protected void UpdateMovement(IMoveAction action) {
			// MoveActions specify where a given actor should be given the tick
			// for the duration of their execution, so we update the position.
			GameObject actor = FindActor(action.GetActorName());
			if (actor != null) {
				GetActorXform(actor, lastTick).Set(actor.transform);
			}
		}
		
		protected virtual void OnGUI() {
			if (!running) return;
			GUI.depth = 0;
			foreach (PlanAction action in executingActions) {
				action.OnGUI(lastTick);
			}
			if (cameraPlan != null) {
				cameraPlan.OnGUI();
			}
		}

		/// <summary>
		/// Returns the GameObject associated with the given actor name
		/// </summary>
		/// <returns>The actor.</returns>
		/// <param name="actor">Actor.</param>
		public GameObject FindActor(string actor) {
			if (actor == null) return null;
			return actors.ContainsKey(actor) ? actors[actor] : null;
		}

		/// <summary>
		/// Registers a GameObject to be associated with an actor's name.
		/// </summary>
		/// <param name="name">Name.</param>
		/// <param name="actor">Actor.</param>
		protected void RegisterActor(string name, GameObject actor) {
			actors.Add(name, actor);
		}
    }
}

