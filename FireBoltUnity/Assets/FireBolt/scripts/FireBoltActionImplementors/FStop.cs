using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    [Obsolete("functionality being folded into ShotFragmentInit", false)]
    public class FStop : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        int fstop;

        string cameraName;
        Vector3 destination;
        CameraBody actor;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public FStop(float startTick, float endTick, string cameraName, int fstop) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.fstop = fstop;
            this.cameraName = cameraName;
        }

        public virtual bool Init()
        {
            actor = GameObject.Find(cameraName).GetComponent<CameraBody>() as CameraBody;
            if (actor == null)
            {
                Debug.LogError("actor name [" + cameraName + "] not found. cannot rotate");
                return false;
            }

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)//we aren't guaranteed a single execution cycle, so move it now and make sure it doesn't move later
                Skip();

            return true;
        }

        public virtual void Execute()
        {

            actor.IndexOfFStop = fstop;
        }

		public virtual void Undo()
		{
		}

        public virtual void Skip()
        {
            actor.IndexOfFStop = fstop;
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
