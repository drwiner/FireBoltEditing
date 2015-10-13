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
		protected Vector3 start;

        Vector3Nullable targetRotation;
        Vector3 rotationChangeRequired;

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
                start = actor.transform.rotation.eulerAngles;
                return true;
            }
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }
			start = actor.transform.rotation.eulerAngles;
        
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
            //the verdict: all actions that have not yet completed at the target time will be undone and then reinitialized and executed

            //let's figure out how far we need to go
            //first we need to convert all the rotation values we have into a standard form.
            //reduce to less than a full rotation
            rotationChangeRequired = new Vector3(targetRotation.X.HasValue ? bindToSemiCircle(targetRotation.X.Value - start.x) : 0,
                                                 targetRotation.Y.HasValue ? bindToSemiCircle(targetRotation.Y.Value - start.y) : 0,
                                                 targetRotation.Z.HasValue ? bindToSemiCircle(targetRotation.Z.Value - start.z) : 0);

            return true;
        }

        private float bindToSemiCircle(float theta)
        {
            theta = theta % 360;
            while (theta > 180)
            {
                theta -= 360;
            }
            while(theta < -180)
            {
                theta += 360;
            }
            return theta;
        }

        public virtual void Execute()
        {
            if (endTick - startTick < 1)
                return;
            //every execute cycle we will get the actor rotation
            Vector3 currentRotation = actor.transform.rotation.eulerAngles;
            
            //if this rotate had something specified on a given axis, we set its position that we are going to add onto to the start position
            //otherwise we will preserve any changes done to actor's orientation subsequent to init of this rotate
            Vector3 newRotation = new Vector3(targetRotation.X.HasValue ? start.x : currentRotation.x,
                                              targetRotation.Y.HasValue ? start.y : currentRotation.y,
                                              targetRotation.Z.HasValue ? start.z : currentRotation.z);

            //how much of our rotate duration has elapsed?
            float percentCompleted = (ElPresidente.currentStoryTime - startTick) / (endTick - startTick);
            
            Vector3 currentRotationAmount = rotationChangeRequired * percentCompleted;

            //add our scaled rotation onto the start/current rotations.  those axes using start also had their total amount set to 0 in the init
            newRotation = newRotation + currentRotationAmount;

            actor.transform.rotation = Quaternion.Euler(newRotation);
            //Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
        }

		public virtual void Undo()
		{
			if (actor != null)
            {
                actor.transform.rotation = Quaternion.Euler(start);
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
