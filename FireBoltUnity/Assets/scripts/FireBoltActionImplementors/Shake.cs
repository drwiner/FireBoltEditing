using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class Shake : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        float shakeValue;

        string actorName;


        ShakeCam actor;


        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Shake(float startTick, float endTick, string actorName, float shakeValue) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.actorName = actorName;
            this.shakeValue = shakeValue;
        }

        public virtual bool Init()
        {
            actor = GameObject.Find(actorName).GetComponent<ShakeCam>() as ShakeCam;
            if (actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot change focus");
                return false;
            }

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)//we aren't guaranteed a single execution cycle, so move it now and make sure it doesn't move later
                Skip();

            return true;
        }

        public virtual void Execute()
        {
            actor.positionShakeSpeed = shakeValue;
            actor.rotationShakeSpeed = shakeValue;
            
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
