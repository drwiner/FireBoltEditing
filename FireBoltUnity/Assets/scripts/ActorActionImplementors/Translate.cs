using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;

namespace Assets.scripts
{
    public class Translate : IActorAction
    {
        float lastUpdateTime;
        float startTick, endTick;
        string actorName;
        Vector3 destination;
        GameObject actor;
        Vector3 requiredVelocity;
        public Translate(float startTick, float endTick, string actorName, Vector3 destination) 
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
                Debug.LogError("actor name [" + actorName + "] not found. cannot move");
            }
            Vector3 direction = (destination - actor.transform.position);
            float moveDuration = endTick - startTick;
            requiredVelocity = new Vector3(direction.x/moveDuration, direction.y/moveDuration, direction.z/moveDuration);
            lastUpdateTime = Time.time;
        }

        public void Execute()
        {
            //move enough to get where we're going before endTick
            float moveTimeElapsed = Time.time - lastUpdateTime;
            Vector3 newPosition = new Vector3(requiredVelocity.x * moveTimeElapsed, 
                                              requiredVelocity.y * moveTimeElapsed, 
                                              requiredVelocity.z * moveTimeElapsed) + actor.transform.position;
            actor.transform.position = newPosition;
            lastUpdateTime = Time.time;
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
