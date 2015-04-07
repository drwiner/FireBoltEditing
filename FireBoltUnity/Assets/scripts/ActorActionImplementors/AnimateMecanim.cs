using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    public class AnimateMecanim : IActorAction
    {
       
        private float startTick;
        private float? endTick;
        private string actorName;
        private GameObject actor;
        private string animName;
        private Animator animator;
        private AnimationClip animation;
        private int animationHash; 

        public AnimateMecanim(float startTick, float? endTick, string actorName, string animName) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.actorName = actorName;
            this.animName = animName;
            animationHash = Animator.StringToHash("trigger");
        }

        public void Init()
        {
            actor = GameObject.Find(actorName);
            if (actor == null) Debug.LogError("actor[" + actorName + "] not found.  cannot animate");

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
            animation = Resources.LoadAssetAtPath<AnimationClip>("Assets/Resources/Animations/" + animName);
            if (!animation)
            {
                Debug.LogError("Missing animation asset");
            }
            animatorOverride["UNTY_Sneak_tk04"] = animation;
           
        }

	    public void Execute () {
		    animator.SetTrigger(animationHash);
	    }

        public void Stop()
        {
 	        
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