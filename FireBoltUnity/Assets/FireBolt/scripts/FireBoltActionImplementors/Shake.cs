using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    //TODO parameterize fully for better control
    public class Shake : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        float shakeValue;

        string cameraName;


        ShakeCam shakeCam;


        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Shake(float startTick, float endTick, string cameraName, float shakeValue) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.cameraName = cameraName;
            this.shakeValue = shakeValue;
        }

        public virtual bool Init()
        {
            GameObject actor = GameObject.Find(cameraName);
            if (actor == null)
            {
                Debug.LogError("actor name [" + cameraName + "] not found. cannot shake");
                return false;
            }

            shakeCam = actor.GetComponent<ShakeCam>() as ShakeCam;
            if (shakeCam == null)
            {
                Debug.LogError(string.Format("camera name [{0}] does not have ShakeCam component",cameraName));
                return false;
            }

            Skip();
            return true;
        }

        public virtual void Execute()
        {
                       
        }

		public virtual void Undo()
		{
            //intentionally blank
        }

        public virtual void Skip()
        {
            shakeCam.positionShakeSpeed = shakeValue;
            shakeCam.rotationShakeSpeed = shakeValue; 
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
