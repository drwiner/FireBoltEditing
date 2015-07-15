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


        ShakeCam shakeCam;


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
            GameObject actor = GameObject.Find(actorName);
            if (actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot shake");
                return false;
            }

            shakeCam = actor.GetComponent<ShakeCam>() as ShakeCam;
            if (shakeCam == null)
            {
                Debug.LogError(string.Format("camera name [{0}] does not have ShakeCam component",actorName));
                return false;
            }

            return true;
        }

        public virtual void Execute()
        {
            shakeCam.positionShakeSpeed = shakeValue;
            shakeCam.rotationShakeSpeed = shakeValue;            
        }

		public virtual void Undo()
		{
            //intentionally blank
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
