using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;
using LN.Utilities;

namespace Assets.scripts
{
    public class Translate : FireBoltAction
    {
        float lastUpdateTime;
        string actorName;
        protected GameObject actor;
        /// <summary>
        /// intended position of the actor when the interval begins
        /// </summary>
        protected Vector3 origin;
        /// <summary>
        /// intended position of the actor when the interval ends
        /// </summary>
        Vector3Nullable destination;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Translate(float startTick, float endTick, string actorName,  Vector3 origin, Vector3Nullable destination) :
            base(startTick, endTick)
        {
            this.actorName = actorName;
            this.origin = origin;
            this.destination = destination;
        }

        public override bool Init()
        {
            if (actor != null)
                return true;
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot move");
                return false;
            }			

            Debug.Log(string.Format("translate [{0}] from [{1}] to [{2}]",actorName,origin,destination));
            return true;
        }

        public override void Execute()
        {
            if (endTick - startTick < 1)
                return;
            float lerpPercent = (ElPresidente.currentStoryTime - startTick)/(endTick-startTick);
            Vector3 lerpd;
            lerpd.x = destination.X.HasValue ? Mathf.Lerp(origin.x,destination.X.Value, lerpPercent) : actor.transform.position.x;
            lerpd.y = destination.Y.HasValue ? Mathf.Lerp(origin.y, destination.Y.Value, lerpPercent) : actor.transform.position.y;
            lerpd.z = destination.Z.HasValue ? Mathf.Lerp(origin.z, destination.Z.Value, lerpPercent) : actor.transform.position.z;
            actor.transform.position = lerpd;
        }

        public override void Undo()
		{
			if (actor != null)
            {
                actor.transform.position = origin;
            }
		}

        public override void Skip()
        {
            Vector3 newPosition;
            newPosition.x = destination.X ?? actor.transform.position.x;
            newPosition.y = destination.Y ?? actor.transform.position.y;
            newPosition.z = destination.Z ?? actor.transform.position.z;
            actor.transform.position = newPosition;
        }

        public override void Stop()
        {
            //nothing to stop
        }
    }
}
