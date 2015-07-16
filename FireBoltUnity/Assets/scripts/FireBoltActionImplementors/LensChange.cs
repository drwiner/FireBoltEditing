using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class LensChange : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        int lensIndex;

        string cameraName;
        Vector3 destination;
        CameraBody camera;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public LensChange(float startTick, float endTick, string cameraName, int lensIndex) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.lensIndex = lensIndex;
            this.cameraName = cameraName; 
        }

        public virtual bool Init()
        {
            camera = GameObject.Find(cameraName).GetComponent<CameraBody>() as CameraBody;
            if (camera == null)
            {
                Debug.LogError(string.Format("camera [{0}] not found. cannot update lensIndex to [{1}]",cameraName,lensIndex));
                return false;
            }
            Debug.Log(string.Format("setting camera [{0}] to lensIndex[{1}]",cameraName,lensIndex));
            camera.IndexOfLens = lensIndex;
            return true;
            //we are not hitting this init again when scrubbing back
        }

        public virtual void Execute()
        {
            camera.IndexOfLens = lensIndex;
        }

		public virtual void Undo()
		{
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
