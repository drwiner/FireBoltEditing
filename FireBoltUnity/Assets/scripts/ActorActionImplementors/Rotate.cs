using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    public class Rotate : IActorAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        string actorName;
        Vector3 destination;
        GameObject actor;
        float requiredVelocity;
        public Rotate(float startTick, float endTick, string actorName, Vector3 destination) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.destination = destination;
        }

        public void Init()
        {
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
            }
            Vector3 direction = (destination - actor.transform.position);
            float rotateDuration = endTick - startTick; //may want to condition this on something 
            float totalRotationRequired = actor.transform.position.GetXZAngleTo(destination);
            requiredVelocity = totalRotationRequired/rotateDuration;
            lastUpdateTime = Time.time * 1000;
        }

        public void Execute()
        {
            float rotateTimeElapsed = Time.time * 1000 - lastUpdateTime;
            Vector3 newRotation = new Vector3(actor.transform.eulerAngles.x,
                              actor.transform.eulerAngles.y + requiredVelocity * rotateTimeElapsed,
                              actor.transform.eulerAngles.z);
            actor.transform.eulerAngles = newRotation;
            lastUpdateTime = Time.time * 1000;
        }

        public void Stop()
        {
            //nothing to stop
        }

        public float StartTick()
        {
            return startTick;
        }

        public float? EndTick()
        {
            return endTick;
        }
    }
}
