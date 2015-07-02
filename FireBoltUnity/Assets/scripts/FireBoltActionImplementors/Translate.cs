using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    public class Translate : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        string actorName;
        protected GameObject actor;
        /// <summary>
        /// actual position of the actor when the interval begins
        /// </summary>
		Vector3 start;
        /// <summary>
        /// intended position of the actor when the interval begins
        /// </summary>
        protected Vector3 origin;
        /// <summary>
        /// intended position of the actor when the interval ends
        /// </summary>
        Vector3 destination;
        /// <summary>
        /// ignore the y parameter in destination and override with current start.y
        /// </summary>
        bool yLock;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Translate(float startTick, float endTick, string actorName, Vector3 origin, Vector3 destination, bool yLock=false) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.origin = origin;
            this.destination = destination;
            this.yLock = yLock;
        }

        public virtual bool Init()
        {
            if (actor != null)
                return true;
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot move");
                return false;
            }
			start = actor.transform.position;
            
            if (yLock)
            {
                destination = new Vector3(destination.x, start.y, destination.z);
                origin = new Vector3(origin.x, start.y, origin.z);
            }

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)
                Skip();

            Debug.Log ("translate from " + start + " to " + destination);
            return true;
        }

        public virtual void Execute()
        {
            actor.transform.position = Vector3.Lerp(start, destination, (ElPresidente.currentTime - startTick)/(endTick-startTick));  
        }

		public virtual void Undo()
		{
			if (actor != null)
            {
                actor.transform.position = start;
                start = origin;
            }
		}

        public virtual void Skip()
        {
            actor.transform.position = destination;
        }

        public virtual void Stop()
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
