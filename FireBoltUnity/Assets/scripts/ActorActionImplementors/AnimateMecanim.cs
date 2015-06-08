using UnityEngine;
using System.Collections;
using UnityEditor;
using CM = CinematicModel;

namespace Assets.scripts
{
    public class AnimateMecanim : IActorAction
    {
       
        private float startTick;
        private float endTick;
        private string actorName;
        private GameObject actor;
        private string animName;
        private Animator animator;
        private AnimationClip animation;
        private int animationHash; 
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
            animationHash = Animator.StringToHash("trigger");
        }

        public bool Init()
        {
            actor = GameObject.Find(actorName);
            if (actor == null)
            {
                Debug.LogError("actor[" + actorName + "] not found.  cannot animate");
                return false;
            }

            animator = actor.GetComponent<Animator>();
            if (animator == null)
            {
                actor.AddComponent<Animator>();
                animator = actor.GetComponent<Animator>();
            }
            animator.applyRootMotion = false;
            //doing all this ever time we start an animation seems expensive. what else can we do?
            AnimatorOverrideController animatorOverride = new AnimatorOverrideController();
            animatorOverride.runtimeAnimatorController = Resources.Load<RuntimeAnimatorController>("AnimatorControllers/Generic");
            animator.runtimeAnimatorController = animatorOverride;
            animation = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Resources/Animations/" + animName);
            AnimationClip oldAnim = AssetDatabase.LoadAssetAtPath<AnimationClip>("Assets/Resources/Animations/humanoid_idle.fbx");
            if (!animation || !oldAnim)
            {
                Debug.LogError("Missing animation asset");
            }
			if (loop) {
				oldAnim.wrapMode = WrapMode.Loop;
			} else
				oldAnim.wrapMode = WrapMode.Once;

            animatorOverride["idle"] = animation;
            return true;
        }

	    public void Execute () 
        {
		    //let it roll
            animator.SetTrigger(animationHash);
	    }

        public void Stop()
        {
            animator.StopPlayback();    
        }

        public float StartTick()
        {
            return startTick;
        }

        public float? EndTick()
        {
            return endTick;
        }
    }
}