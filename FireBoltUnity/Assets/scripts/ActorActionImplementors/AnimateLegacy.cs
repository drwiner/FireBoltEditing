using UnityEngine;
using System.Collections;

namespace Assets.scripts
{
    //concrete
    public class AnimateLegacy : IActorAction
    {

        private string animName;
        private string actorName;
        public AnimateLegacy(float startTick, float? endTick, string actorName,
            IActorAction nestedAction, string animName)             
        {
            this.animName = animName;
            this.actorName = actorName;
            

        }


        public bool Init()
        {
            throw new System.NotImplementedException();
        }

        public void Execute()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public float StartTick()
        {
            throw new System.NotImplementedException();
        }

        public float? EndTick()
        {
            throw new System.NotImplementedException();
        }

		public void Undo()
		{
			// joining the cool kids' club
			throw new System.NotImplementedException();
		}
    }
}