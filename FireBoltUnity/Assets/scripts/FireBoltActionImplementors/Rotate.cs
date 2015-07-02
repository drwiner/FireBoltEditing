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
        
            target = Quaternion.Euler(0, targetDegrees, 0);

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)//we aren't guaranteed a single execution cycle, so move it now and make sure it doesn't move later
                Skip();

            return true;
        }

        public void Execute()
        {
            actor.transform.rotation = Quaternion.Lerp (start, target, (ElPresidente.currentTime - startTick) / (endTick - startTick));
            //Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
        }

		public void Undo()
		{
			if (actor != null)
            {
                actor.transform.rotation = start;
            }
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
