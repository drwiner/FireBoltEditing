using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class Focus : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        int lensIndex;

        string actorName;
        string targetName;
        Transform focusLocation;
        CameraBody actor;
        GameObject target;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Focus(float startTick, float endTick, string actorName, string targetName) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.actorName = actorName;
            this.targetName = targetName;
        }

        public virtual bool Init()
        {
            actor = GameObject.Find(actorName).GetComponent<CameraBody>() as CameraBody;
            if (actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot change focus");
                return false;
            }

            target = GameObject.Find(targetName);
            if (actor == null)
            {
                Debug.LogError("actor name [" + targetName + "] not found. cannot change focus");
                return false;
            }

            focusLocation = target.transform;

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)//we aren't guaranteed a single execution cycle, so move it now and make sure it doesn't move later
                Skip();

            return true;
        }

        public virtual void Execute()
        {
            actor.FocusTransform = focusLocation;
        }

		public virtual void Undo()
		{
			if (actor != null)
            {
               
            }
		}

        public virtual void Skip()
        {
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
