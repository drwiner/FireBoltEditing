using LN.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.scripts
{
    public class ShotFragmentInit : IFireBoltAction
    {
        //passed in params
        private float startTick;
        private string anchor=string.Empty;
        private float? height;//TODO implement height as ground and actor-based value
        private string cameraName;
        private Angle cameraAngle;

        //parameter grounding
        Vector2? anchorPosition;
        Vector3 eulerAngles;

        //saved camera values
        GameObject camera;
        Quaternion previousCameraOrientation = Quaternion.identity;
        Vector3 previousCameraPosition = Vector3.zero;

        //final camera values
        Quaternion newCameraOrientation;
        Vector3 newCameraPosition;


        public ShotFragmentInit(float startTick, string cameraName, string anchor, float? height, Angle cameraAngle)
        {
            this.startTick = startTick;
            this.cameraName = cameraName;
            this.anchor = anchor;
            this.height = height;
            this.cameraAngle = cameraAngle;
        }

        public bool Init()
        {
            //save values for undo
            camera = GameObject.Find(cameraName) as GameObject;
            if (camera == null)
            {
                Debug.LogError(string.Format("could not find camera[{0}] at time d:s[{1}:{2}].  This is really bad.", 
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                return false;
            }

            previousCameraOrientation = camera.transform.rotation;
            previousCameraPosition = camera.transform.position;

            //ground parameters
            calculateAnchor(anchor, out anchorPosition);
            calculateCameraAngle();

            //sort out what wins where and assign to final camera properties
            //start with previous camera properties in case nothing fills them in
            newCameraPosition = previousCameraPosition;
            newCameraOrientation = previousCameraOrientation;


            return true;
        }

        /// <summary>
        /// function of position and angleSetting.  this needs our framing to find x-z position if it's not specified.
        /// in the simplest case, x,y,z are already set by the author and we simply set the rotation we want.
        /// </summary>
        private void calculateCameraAngle()
        {

        }

        private void calculateAnchor(string anchor, out Vector2? anchorPosition)
        {
            anchorPosition = null;
            //if there's nothing there, then nothing to ground to
            if (string.IsNullOrEmpty(anchor)) return;
            Vector2 planarCoords;
            if (anchor.TryParsePlanarCoords(out planarCoords))
            {
                //we can read the anchor string as planar coords
                anchorPosition = planarCoords;
            }
            else
            {
                //we can't read anchor string as planar coords.  hopefully this is the name of an actor
                GameObject actorToAnchorOn = GameObject.Find(anchor) as GameObject;

                if (actorToAnchorOn == null)                    
                {
                    //sadly there is no such thing.  we should complain and then try to get on with business
                    Debug.LogError(string.Format("anchor actor [{0}] not found at time d:s[{1}:{2}].  calculating anchor freely.", 
                        anchor, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                    return;
                }
                Vector3 actorPosition = actorToAnchorOn.transform.position;
                anchorPosition = new Vector2(actorPosition.x, actorPosition.z);
            }
        }

        public void Execute()
        {
            //nothing to see here.  this is all instant
        }

        public void Stop()
        {
            //nothing to do and nothing to stop
        }

        public float StartTick()
        {
            return startTick;
        }

        public float EndTick()
        {
            return startTick;
        }

        public void Undo()
        {
            camera.transform.position = previousCameraPosition;
            camera.transform.rotation = previousCameraOrientation;
        }

        public void Skip()
        {
            //since this action always happens instantaneously we can assume that the 
            //skip will get run anytime it's selected for addition in the 
            //executing queue in el Presidente
            camera.transform.position = newCameraPosition;
            camera.transform.rotation = newCameraOrientation;
        }

    }
}
