﻿using UnityEngine;
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

        string actorName;
        Vector3 destination;
        CameraBody actor;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public LensChange(float startTick, float endTick, string actorName, int lensIndex) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.lensIndex = lensIndex;
            this.actorName = actorName;
        }

        public virtual bool Init()
        {
            actor = GameObject.Find(actorName).GetComponent<CameraBody>() as CameraBody;
            if (actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)//we aren't guaranteed a single execution cycle, so move it now and make sure it doesn't move later
                Skip();

            return true;
        }

        public virtual void Execute()
        {
            actor.IndexOfLens = lensIndex;
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