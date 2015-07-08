using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LN.Utilities;

namespace Assets.scripts
{
    public class RotateRelative : Rotate
    {
        private string trackedActorName;
        private GameObject trackedActor;
        private Vector3 trackedPositionInit;
        private Vector3 trackedPositionLast;
        private bool[] dimensionLock = {false,false,false};
        private static readonly float ROTATION_SPEED_MAX = .75f;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackedActorName"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="xLock">rotate about x axis</param>
        /// <param name="yLock">rotate about y axis</param>
        /// <param name="zLock">rotate about z axis</param>
        public RotateRelative(string trackedActorName, float startTick, float endTick, string actorName, bool xLock, bool yLock, bool zLock) :
            base(startTick, endTick, actorName, 0f )
        {
            this.trackedActorName = trackedActorName;
            dimensionLock[0] = xLock;
            dimensionLock[1] = yLock;
            dimensionLock[2] = zLock;
        }

        public override bool Init()
        {
            trackedActor = GameObject.Find(trackedActorName);
            if (trackedActor != null)
            {
                trackedPositionInit = trackedActor.transform.position;
                trackedPositionLast = trackedPositionInit;
                if (base.Init())
                {                    
                    return true;
                }                              
            }                
            return false;
        }

        public override void Execute()
        {
            //TODO check interactions in multiple axes with slerping

            Vector3 trackedPositionCurrent = trackedActor.transform.position;

            Vector3 direction;
            direction.x = !dimensionLock[0] ? 0 : trackedPositionCurrent.x - this.actor.transform.position.x;
            direction.y = !dimensionLock[1] ? 0 : trackedPositionCurrent.y - this.actor.transform.position.y;
            direction.z = !dimensionLock[2] ? 0 : trackedPositionCurrent.z - this.actor.transform.position.z;
            direction = direction.normalized;

            Quaternion lookRotation = Quaternion.LookRotation(direction, Vector3.up);
            this.actor.transform.rotation = Quaternion.Slerp(this.actor.transform.rotation, lookRotation, ROTATION_SPEED_MAX * Time.deltaTime);

            trackedPositionLast = trackedPositionCurrent;
        }


        //assumes this action sorts after the move that it mirrors.  fails when we track over multiple actor moves.  
        //should implement sorting on executing actions to put relative movements last || just fix this so it's not so janky
        public override void Skip()
        {
            Vector3 direction;
            direction.x = dimensionLock[0] ? trackedActor.transform.position.x - this.actor.transform.position.x : this.actor.transform.position.x;
            direction.y = dimensionLock[1] ? trackedActor.transform.position.y - this.actor.transform.position.y : this.actor.transform.position.y;
            direction.z = dimensionLock[2] ? trackedActor.transform.position.z - this.actor.transform.position.z : this.actor.transform.position.z;
            direction = direction.normalized;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            this.actor.transform.rotation = lookRotation;
        }

        public override void Undo()
        {
            this.actor.transform.rotation = this.start;
        }
    }
}
