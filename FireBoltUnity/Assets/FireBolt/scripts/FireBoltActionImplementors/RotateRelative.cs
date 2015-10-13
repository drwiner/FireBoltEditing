using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using LN.Utilities;

namespace Assets.scripts
{
    //used to override rotate, but we minimally used underlying code, and it was just confusing.
    public class RotateRelative : FireBoltAction
    {
        private string trackedActorName;
        private GameObject trackedActor;      

        private string actorName;
        private GameObject actor;
        private Vector3 startOrientation;

        private bool initialized = false;

        private bool[] rotationAxes = {false,false,false};
        private bool instant = false;
        private float rotationSpeed = .75f; //seems like degrees per tick

        /// <summary>
        /// 
        /// </summary>
        /// <param name="trackedActorName"></param>
        /// <param name="startTick"></param>
        /// <param name="endTick"></param>
        /// <param name="actorName"></param>
        /// <param name="xLock">rotate about x axis</param>
        /// <param name="yLock">rotate about y axis</param>
        /// <param name="rotateZ">rotate about z axis</param>
        public RotateRelative(string trackedActorName, float startTick, float endTick, string actorName, bool rotateX, bool rotateY, bool rotateZ) :
            base(startTick, endTick)
        {
            this.trackedActorName = trackedActorName;
            this.actorName = actorName;
            rotationAxes[0] = rotateX;
            rotationAxes[1] = rotateY;
            rotationAxes[2] = rotateZ;
        }

        public override bool Init()
        {
            if (initialized)
                return true;                

            //get our actor
            actor = GameObject.Find(actorName);                                           
            if (actor == null)
            {
                Debug.LogError("actor name [" + actorName + "] not found. cannot rotate");
                return false;
            }
            startOrientation = actor.transform.rotation.eulerAngles;

            //find actor that should be tracked
            trackedActor = GameObject.Find(trackedActorName);
            if (trackedActor == null)
            {
                Debug.LogError(string.Format("actor to track [{0}] by actor [{1}] not found.  cannot rotate",trackedActorName, actorName));
                return false;
            }

            initialized = true;
            return true;
        }

        public override void Execute()
        {
            //capture updated tracked position
            Vector3 trackedPositionCurrent = trackedActor.transform.position;

            //find the shortest way to get to him
            //get current position for our actor
            Vector3 actorPosition = actor.transform.position;

            //get the direction from our actor to the tracked actor.  this is the vector along which we are trying to align
            //apply no direction along axes we are not tracking
            Vector3 targetDirection;
            if (rotationAxes[1] || rotationAxes[2])
                targetDirection.x = trackedPositionCurrent.x - actorPosition.x;
            else
                targetDirection.x = 0;
            if (rotationAxes[1] || rotationAxes[2])
                targetDirection.y = trackedPositionCurrent.y - actorPosition.y;
            else
                targetDirection.y = 0;
            if (rotationAxes[0] || rotationAxes[1])
                targetDirection.z = trackedPositionCurrent.z - actorPosition.z;
            else
                targetDirection.z = 0;

            //renormalize since we dropped a dimension most likely
            targetDirection.Normalize();

            //now we have a normalized vector from our actor to our trackedActor
            //that lies in the plane that we can use to describe a rotation about our allowed axes
            //convert from a vector to a rotation value
            Vector3 currentActorOrientation = actor.transform.rotation.eulerAngles;
            Vector3 currentRotationAdditive = Vector3.zero;
            if (rotationAxes[0])
            {   
                //get a number that is where we want to go
                float finalRotationValue = Mathf.Atan2(targetDirection.z, targetDirection.y);
                //get a number that we add to our current rotation to get to where we want to go
                float rotationDifferential = (finalRotationValue - currentActorOrientation.x);
                //scale that number by our allowable rotation speed and add to our current orientation
                currentRotationAdditive.x = currentRotationAdditive.x + rotationSpeed * Time.deltaTime * rotationDifferential;
            }
            if(rotationAxes[1])
            {
                float finalRotationValue = Mathf.Atan2(targetDirection.z, targetDirection.x);
                float rotationDifferential = (finalRotationValue - currentActorOrientation.y);
                currentRotationAdditive.y = currentRotationAdditive.y + rotationSpeed * Time.deltaTime * rotationDifferential;
            }
            if(rotationAxes[2])
            {
                float finalRotationValue = Mathf.Atan2(targetDirection.z, targetDirection.x);
                float rotationDifferential = (finalRotationValue - currentActorOrientation.z);
                currentRotationAdditive.z = currentRotationAdditive.z + rotationSpeed * Time.deltaTime * rotationDifferential;
            }

            actor.transform.rotation = Quaternion.Euler(actor.transform.rotation.eulerAngles + currentRotationAdditive);


            //Quaternion lookRotation = Quaternion.LookRotation(targetDirection, Vector3.up);
            //float rotation = rotationSpeed * Time.deltaTime;
            //this.actor.transform.rotation = Quaternion.Slerp(this.actor.transform.rotation, lookRotation, rotation);
        }


        //assumes this action sorts after the move that it mirrors.  this works well for cameras tracking things and 
        //less well for actors tracking other actors since camera actions are forced to play second        
        public override void Skip()
        {
            Vector3 direction = Vector3.zero;
            //direction.x = dimensionLock[0] ? trackedActor.transform.position.x - this.actor.transform.position.x : this.actor.transform.position.x;
            //direction.y = dimensionLock[1] ? trackedActor.transform.position.y - this.actor.transform.position.y : this.actor.transform.position.y;
            //direction.z = dimensionLock[2] ? trackedActor.transform.position.z - this.actor.transform.position.z : this.actor.transform.position.z;
            //direction = direction.normalized;

            Quaternion lookRotation = Quaternion.LookRotation(direction);
            this.actor.transform.rotation = lookRotation;
        }

        public override void Undo()
        {
            this.actor.transform.rotation = Quaternion.Euler(startOrientation);
        }

        public override void Stop()
        {
            
        }
    }
}
