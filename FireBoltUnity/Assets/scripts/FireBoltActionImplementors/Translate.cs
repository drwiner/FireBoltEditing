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
        GameObject actor;
        /// <summary>
        /// actual position of the actor when the interval begins
        /// </summary>
		Vector3 start;
        /// <summary>
        /// intended position of the actor when the interval begins
        /// </summary>
        Vector3 origin;
        /// <summary>
        /// intended position of the actor when the interval ends
        /// </summary>
        Vector3 destination;
        /// <summary>
        /// do we ignore the y parameter in destination
        /// </summary>
        bool yLock;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public override string ToString ()
        {
            return "Translate " + actorName + " from " + origin + " to " + destination;
        }

        public Translate(float startTick, float endTick, string actorName, Vector3 origin, Vector3 destination) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.origin = origin;
            this.destination = destination;
            this.yLock = yLock;
        }

        public bool Init()
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
                destination = new Vector3(destination.x, start.y, destination.z);
            Debug.Log ("translate from " + start + " to " + destination);
            return true;
        }

        public void Execute()
        {
            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)           
                actor.transform.position = destination;
            else
                actor.transform.position = Vector3.Lerp(start, destination, (ElPresidente.currentTime - startTick)/(endTick-startTick));  
        }

		public void Undo()
		{
			if (actor != null)
            {
                actor.transform.position = start;
                start = origin;
            }
		}

        public void Skip()
        {
            actor.transform.position = destination;
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
