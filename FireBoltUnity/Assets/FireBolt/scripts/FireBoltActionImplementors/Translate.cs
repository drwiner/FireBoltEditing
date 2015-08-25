﻿using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;
using LN.Utilities;

namespace Assets.scripts
{
    public class Translate : IFireBoltAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        string actorName;
        protected GameObject actor;
        /// <summary>
        /// actual position of the actor when the interval begins
        /// </summary>
		Vector3 start;
        /// <summary>
        /// intended position of the actor when the interval begins
        /// </summary>
        protected Vector3 origin;
        /// <summary>
        /// intended position of the actor when the interval ends
        /// </summary>
        Vector3Nullable destination;
        /// <summary>
        /// ignore origin and reset from start
        /// </summary>
        bool unknownOrigin;

        bool controlsTime;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Translate(float startTick, float endTick, string actorName,  Vector3 origin, Vector3Nullable destination, bool unknownOrigin=false, bool controlsTime=false) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.origin = origin;
            this.destination = destination;
            this.unknownOrigin = unknownOrigin;
            this.controlsTime = controlsTime;
        }

        public void SetDestination(Vector3Nullable destination)
        {
            this.destination = destination;
        }

        public virtual bool Init()
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

            //time sync :\
            //if (controlsTime &&// i don't like this much
            //   Mathf.Abs(startTick - ElPresidente.currentTime) > ElPresidente.MILLIS_PER_FRAME)
            //    ElPresidente.Instance.goTo(startTick);


            if (unknownOrigin)
                origin = start;

            if (endTick - startTick < ElPresidente.MILLIS_PER_FRAME)
                Skip();

            Debug.Log(string.Format("translate {0} from {1} to {2}",actorName,origin,destination));
            return true;
        }

        public virtual void Execute()
        {
            if (endTick - startTick < 1)
                return;
            float lerpPercent = (ElPresidente.currentTime - startTick)/(endTick-startTick);
            Vector3 lerpd;
            lerpd.x = destination.X.HasValue ? Mathf.Lerp(start.x,destination.X.Value, lerpPercent) : actor.transform.position.x;
            lerpd.y = destination.Y.HasValue ? Mathf.Lerp(start.y, destination.Y.Value, lerpPercent) : actor.transform.position.y;
            lerpd.z = destination.Z.HasValue ? Mathf.Lerp(start.z, destination.Z.Value, lerpPercent) : actor.transform.position.z;
            actor.transform.position = lerpd;
        }

		public virtual void Undo()
		{
			if (actor != null)
            {
                actor.transform.position = start;
                start = origin;
            }
		}

        public virtual void Skip()
        {
            Vector3 newPosition;
            newPosition.x = destination.X.HasValue ? destination.X.Value : start.x;
            newPosition.y = destination.Y.HasValue ? destination.Y.Value : start.y;
            newPosition.z = destination.Z.HasValue ? destination.Z.Value : start.z;
            actor.transform.position = newPosition;
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