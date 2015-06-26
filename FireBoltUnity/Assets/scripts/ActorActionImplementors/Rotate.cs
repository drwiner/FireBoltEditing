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
		Quaternion start;



        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="targetDegrees">must be in unity axes</param>
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
			start = actor.transform.rotation;
            targetDegrees = convertSourceEngineToUnityRotation(targetDegrees);

            float rotateDuration = endTick - startTick;
            float totalRotationRequired = Mathf.Abs(actor.transform.rotation.eulerAngles.y - targetDegrees);            
            requiredVelocity = totalRotationRequired/rotateDuration;

            target = Quaternion.Euler(0, targetDegrees, 0);
            lastUpdateTime = ElPresidente.currentTime;
            return true;
        }

        public void Execute()
        {
            float rotateTimeElapsed = ElPresidente.currentTime - lastUpdateTime;
            actor.transform.rotation = Quaternion.RotateTowards(actor.transform.rotation, target, requiredVelocity * rotateTimeElapsed);
            lastUpdateTime = ElPresidente.currentTime;
            //Debug.DrawRay(actor.transform.position + Vector3.up, actor.transform.forward,Color.magenta);
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

		public void Undo()
		{
			if (actor != null)
			    actor.transform.rotation = start;
		}

        public void Skip()
        {
            actor.transform.rotation = target;
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
