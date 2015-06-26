using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    public class Rotate : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        string actorName;
        Vector3 destination;
        float targetDegrees;
        GameObject actor;
        float requiredVelocity;
        float stepSize;
        Quaternion target;
		Quaternion start;



        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="targetDegrees">must be in unity axes</param>
        public Rotate(float startTick, float endTick, string actorName, float targetDegrees) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.targetDegrees = targetDegrees;
        }

        public bool Init()
        {
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }
			start = actor.transform.rotation;

            float rotateDuration = endTick - startTick > 0 ? endTick - startTick : 1;            
            target = Quaternion.Euler(0, targetDegrees, 0);
            
            if (rotateDuration < ElPresidente.MILLIS_PER_FRAME)//we aren't guaranteed a single execution cycle, so move it now and make sure it doesn't move later
            {
                actor.transform.rotation = target;
                requiredVelocity = 0;
            }
            else
            {
                float totalRotationRequired = Mathf.Abs(actor.transform.rotation.eulerAngles.y - targetDegrees);
                requiredVelocity = totalRotationRequired / rotateDuration;
            }          

            lastUpdateTime = ElPresidente.currentTime;
            return true;
        }

        public void Execute()
        {
            float rotateTimeElapsed = ElPresidente.currentTime - lastUpdateTime;
            actor.transform.rotation = Quaternion.RotateTowards(actor.transform.rotation, target, requiredVelocity * rotateTimeElapsed);
            lastUpdateTime = ElPresidente.currentTime;
            //Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
        }

		public void Undo()
		{
			if (actor != null)
			    actor.transform.rotation = start;
		}

        public void Skip()
        {
            actor.transform.rotation = target;
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
