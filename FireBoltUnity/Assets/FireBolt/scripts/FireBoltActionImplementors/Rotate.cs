using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities;

namespace Assets.scripts
{
    public class Rotate : IFireBoltAction
    {
        float startTick, endTick;
        string actorName;
       
        protected GameObject actor;
        Quaternion target;
		protected Quaternion start;

        Vector3Nullable targetRotation;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public override string ToString ()
        {
            return "Rotate " + actorName + " from " + start + " to " + target;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="targetRotation">must be in unity axes</param>
        public Rotate(float startTick, float endTick, string actorName, Vector3Nullable targetRotation) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.targetRotation = targetRotation;
        }

        public virtual bool Init()
        {
            if (actor != null)
            {
                start = actor.transform.rotation;
                return true;
            }
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }
			start = actor.transform.rotation;
        

            //if i fill in zeros here, there are issues
            //i'm saying that my orientation is 0 about the x and 0 about the z, so i can't tilt or yaw
            //that was great for planar actors, but it's not general enough
            //now we get values(or lack thereof) for all 3 axes from constructor.
            //I have to devise a mechanism that varies only those axes and allows variation for other by other rotate objects
            //this seems to indicate that i should be getting and setting my quaternion for actor rotation all within the execute.
            //how do i track how close we are to completed for a given rotation?

            //target = Quaternion.Euler(0, targetRotation, 0);

            //this line of reasoning sends me back toward what i was doing originally where i did interpolation myself.
            //then i was keeping track of where I should be using some additive method, but that no longer works with how 
            //we are scrubbing and setting time.
            // i must away to el Presidente and discover what happens when I scrub back to the middle of an action
            return true;
        }

        public virtual void Execute()
        {
            if (endTick - startTick < 1)
                return;
            actor.transform.rotation = Quaternion.Lerp (start, target, (ElPresidente.currentStoryTime - startTick) / (endTick - startTick));
            //Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
        }

		public virtual void Undo()
		{
			if (actor != null)
            {
                actor.transform.rotation = start;
            }
		}

        public virtual void Skip()
        {
            Vector3 actorTransformEulerAngles = actor.transform.rotation.eulerAngles;
            //for any unspecified values in targetRotation, set that axis angle to current value
            actor.transform.rotation = Quaternion.Euler(new Vector3(targetRotation.X ?? actorTransformEulerAngles.x,
                                                                    targetRotation.Y ?? actorTransformEulerAngles.y,
                                                                    targetRotation.Z ?? actorTransformEulerAngles.z));
        }

        public void Stop()
        {
            //nothing to stop
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
