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
        Vector3 destination;
        Vector3 origin;
        GameObject actor;
		Vector3 start;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Translate(float startTick, float endTick, string actorName, Vector3 origin, Vector3 destination) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.origin = origin;
            this.destination = destination;
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
            return true;
        }

        public void Execute()
        {
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
