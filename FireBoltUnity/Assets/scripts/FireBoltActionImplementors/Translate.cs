using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

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
        /// ignore the given parameter when lerping to destination 
        /// </summary>
        bool xLock, yLock, zLock;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Translate(float startTick, float endTick, string actorName, Vector3 origin, Vector3 destination, bool xLock=false, bool yLock=false, bool zLock=false) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.origin = origin;
            this.destination = destination;
            this.xLock = xLock;
            this.yLock = yLock;
            this.zLock = zLock;
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

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)
                Skip();

            Debug.Log ("translate from " + start + " to " + destination);
            return true;
        }

        public virtual void Execute()
        {
            float lerpPercent = (ElPresidente.currentTime - startTick)/(endTick-startTick);
            Vector3 lerpd = actor.transform.position;
            if(!xLock)
                lerpd.x = Mathf.Lerp(start.x,destination.x, lerpPercent);
            if(!yLock)
                lerpd.y = Mathf.Lerp(start.y,destination.y, lerpPercent);
            if(!zLock)
                lerpd.z = Mathf.Lerp(start.z, destination.z, lerpPercent);
            actor.transform.position = lerpd;
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
            Vector3 newPosition = start;
            if (!xLock)
                newPosition.x = destination.x;
            if (!yLock)
                newPosition.y = destination.y;
            if (!zLock)
                newPosition.z = destination.z;
            actor.transform.position = newPosition;
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
