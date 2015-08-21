using UnityEngine;
using System.Collections;
using CM = CinematicModel;
//using UnityEditor;

namespace Assets.scripts
{
    public class AnimateMecanim : IFireBoltAction
    {
       
        private float startTick;
        private float endTick;
        private string actorName;
        private GameObject actor;
        private string animName;
        private Animator animator;
        private AnimationClip animation;
		private AnimationClip oldClip;
		AnimatorOverrideController animatorOverride;
        private int playTriggerHash,stopTriggerHash; 
		private bool loop;

        public static bool ValidForConstruction(string actorName, CM.Animation animation)
        {
            if (string.IsNullOrEmpty(actorName) || animation == null || string.IsNullOrEmpty(animation.FileName))
                return false;
            return true;
        }

        public AnimateMecanim(float startTick, float endTick, string actorName, string animName, bool loop) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.actorName = actorName;
            this.animName = animName;
			this.loop = loop;
            playTriggerHash = Animator.StringToHash("play");
            stopTriggerHash = Animator.StringToHash("stop");
        }

        public bool Init()
        {
			if (animatorOverride != null)
			{
				animatorOverride["idle"] = animation;
                animator.runtimeAnimatorController = animatorOverride;
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

            //animation = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Resources/Animations/" + animName);
            AnimationClip oldAnim =null;//= AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Resources/Animations/humanoid_idle.fbx");
            
            if (!animation || !oldAnim)
            {
                Debug.LogError("Missing animation asset");
            }
			if (loop) {
				animation.wrapMode = WrapMode.Loop;
			} else
				animation.wrapMode = WrapMode.Once;
			oldClip = animatorOverride ["idle"];
            animatorOverride["idle"] = animation;
            Debug.Log ("duration " + animation.averageDuration);
            return true;
        }

		public void Undo()
		{
			if (animatorOverride != null)
			    animatorOverride["idle"] = oldClip;
		}

        public void Skip()
        {
            animator.SetTrigger(stopTriggerHash);
            animatorOverride["idle"] = oldClip;
        }

	    public void Execute () 
        {
		    //let it roll
            //animator.SetTrigger(playTriggerHash);
            float at = Mathf.Repeat ((ElPresidente.currentTime - startTick)/1000, animation.averageDuration);
            animator.CrossFade( "animating", 0, 0, at/animation.averageDuration );

	    }

        public void Stop()
        {
            animator.SetTrigger(stopTriggerHash);
            animatorOverride["idle"] = oldClip;
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