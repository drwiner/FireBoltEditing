using UnityEngine;
using System.Collections;
using CM = CinematicModel;

namespace Assets.scripts
{
    public class AnimateMecanim : IFireBoltAction
    {

        private float startTick;
        private float endTick;
        private string actorName;
        private GameObject actor;
        private string animName;
        private string stateName;
        private Animator animator;
        private AnimationClip animation;
        private AnimationClip state;
        AnimatorOverrideController animatorOverride;
        private int playTriggerHash, stopTriggerHash;
        private bool loop;
        private static readonly string animationToOverride = "_87_a_U1_M_P_idle_Neutral__Fb_p0_No_1";
        private static readonly string stateToOverride = "state";
        bool assignEndState = false;

        public static bool ValidForConstruction(string actorName, CM.Animation animation)
        {
            if (string.IsNullOrEmpty(actorName) || animation == null || string.IsNullOrEmpty(animation.FileName))
                return false;
            return true;
        }

        public AnimateMecanim(float startTick, float endTick, string actorName, string animName, bool loop, string endingName) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.actorName = actorName;
            this.animName = animName;
			this.loop = loop;
            this.stateName = endingName; 
            playTriggerHash = Animator.StringToHash("play");
            stopTriggerHash = Animator.StringToHash("stop");
        }

        public bool Init()
        {
			if (actor != null && animatorOverride != null)
			{
				animatorOverride[animationToOverride] = animation;
                animator.runtimeAnimatorController = animatorOverride;
                if (assignEndState)
                    animatorOverride[stateToOverride] = state;
				return true;
			}
            actor = GameObject.Find(actorName);
            if (actor == null)
            {
                Debug.LogError("actor[" + actorName + "] not found.  cannot animate");
                return false;
            }

            animator = actor.GetComponent<Animator>();
            if (animator == null)
            {
                animator = actor.AddComponent<Animator>();
            }
            animator.applyRootMotion = false;
            //doing all this every time we start an animation seems expensive. what else can we do?
            animatorOverride = new AnimatorOverrideController();
            animatorOverride.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimatorControllers/Generic");
            animator.runtimeAnimatorController = animatorOverride;

            if(ElPresidente.Instance.GetActiveAssetBundle().Contains(animName))
            {
                animation = ElPresidente.Instance.GetActiveAssetBundle().LoadAsset<AnimationClip>(animName);
                if (ElPresidente.Instance.GetActiveAssetBundle().Contains(stateName) && !stateName.Equals("")) //added by DrW
                {
                    assignEndState = true;
                    state = ElPresidente.Instance.GetActiveAssetBundle().LoadAsset<AnimationClip>(stateName);
                    if (state == null)
                    {
                        Debug.LogError(string.Format("unable to find animation [{0}] in asset bundle[{1}]", stateName, ElPresidente.Instance.GetActiveAssetBundle().name));
                        if (state == null) return false;
                    }
                }
                if (animation == null)
                {
                    Debug.LogError(string.Format("unable to find animation [{0}] in asset bundle[{1}]",animName, ElPresidente.Instance.GetActiveAssetBundle().name));
                    return false;
                }
            }
            

            //TODO build animation controller dynamically and avoid having to do overriding.  
            //This allows us to avoid packaging a default animation that may not be valid 
            //for the rig of a given actor that it's supposed to be played on.
            //It would also remove the reconfigure on download issue of the animation type and name.

            //we can't do this!  creating new animator controllers is a function only available in the UnityEditor library ftl
            //can i just not load the old animation? indeed we can do just not that!
            if (!animation) 
            {
                Debug.LogError("Missing animation asset");
            }
            if (assignEndState && !state)
            {
                Debug.LogError("Missing state asset");
            }

			if (loop) {
				animation.wrapMode = WrapMode.Loop;
			} else
				animation.wrapMode = WrapMode.Once;			
            animatorOverride[animationToOverride] = animation;
            if (assignEndState)
                animatorOverride[stateToOverride] = state;
            return true;
        }

		public void Undo()
		{
		}

        public void Skip()
        {
            animator.SetTrigger(stopTriggerHash);
        }

	    public void Execute () 
        {
		    //let it roll          
            float at = Mathf.Repeat ((ElPresidente.currentStoryTime - startTick)/1000, animation.length);
            animator.CrossFade( "animating", 0, 0, at/animation.length);
	    }

        public void Stop()
        {
            animator.SetTrigger(stopTriggerHash);
        }

        public float StartTick()
        {
            return startTick;
        }

        public float EndTick()
        {
            return endTick;
        }
    }
}