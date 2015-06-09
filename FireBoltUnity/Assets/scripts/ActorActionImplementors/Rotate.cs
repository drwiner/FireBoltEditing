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
        float targetDegrees;
        GameObject actor;
        float requiredVelocity;
        float stepSize;
        Quaternion target;


        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Rotate(float startTick, float endTick, string actorName, float targetDegrees) 
        {
            this.startTick = startTick;
            this.actorName = actorName;
            this.endTick = endTick;
            this.targetDegrees = targetDegrees;
        }

        public bool Init()
        {
            actor = GameObject.Find(actorName);
            if(actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }
            
            targetDegrees = convertSourceEngineToUnityRotation(targetDegrees);

            float rotateDuration = endTick - startTick;
            float totalRotationRequired = Mathf.Abs(actor.transform.rotation.eulerAngles.y - targetDegrees);            
            requiredVelocity = totalRotationRequired/rotateDuration;

            target = Quaternion.Euler(0, targetDegrees, 0);
            lastUpdateTime = Time.time * 1000;
            return true;
        }

        public void Execute()
        {
            float rotateTimeElapsed = Time.time * 1000 - lastUpdateTime;
            //Vector3 newRotation = new Vector3(actor.transform.eulerAngles.x,
            //                  actor.transform.eulerAngles.y + requiredVelocity * rotateTimeElapsed,
            //                  actor.transform.eulerAngles.z);
            //actor.transform.eulerAngles = newRotation;
            actor.transform.rotation = Quaternion.RotateTowards(actor.transform.rotation, target, requiredVelocity * rotateTimeElapsed);
            lastUpdateTime = Time.time * 1000;
            Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
        }

        private float convertSourceEngineToUnityRotation(float sourceDegrees)
        {
            float unityDegrees = -sourceDegrees + 90 % 360;
            while(unityDegrees > 180)
            {
                unityDegrees -= 360;
            }
            while(unityDegrees < -180)
            {
                unityDegrees += 360;
            }
            return unityDegrees;
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
