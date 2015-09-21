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
        private float startTick,endTick;
        private bool initialized = false;
        private string anchor=string.Empty;
        private float? height;//TODO implement height specification in oshmirto
        private string cameraName; //this is actually going to manipulate the rig most likely, but it doesn't matter much from in here
        private CameraBody cameraBody; //need a reference to this guy for setting fstop and lens
        private string lensName;
        private string fStopName;
        private Angle cameraAngle;
        private string focusTarget;

        //parameter grounding
        
        Vector3Nullable tempCameraPosition;
        Vector3Nullable tempCameraOrientation;

        //saved camera values
        GameObject camera;
        Quaternion previousCameraOrientation = Quaternion.identity;
        Vector3 previousCameraPosition = Vector3.zero;
        ushort previousLensIndex;
        ushort previousFStopIndex;

        //final camera values
        Quaternion newCameraOrientation;
        Vector3 newCameraPosition;
        ushort? lensIndex;
        ushort? fStopIndex;
        float? newFocusDistance;

        public ShotFragmentInit(float startTick, float endTick, string cameraName, string anchor, float? height, string lensName, string fStopName, Angle cameraAngle, string focusTarget)
        {
            this.startTick = startTick;
            this.endTick = endTick;//used in querying for direction over the shot.  not in setting end of this init action
            this.cameraName = cameraName;
            this.anchor = anchor;
            this.height = height;
            this.lensName = lensName;
            this.fStopName = fStopName;
            this.cameraAngle = cameraAngle;
            this.focusTarget = focusTarget;
        }

        public bool Init()
        {
            if(initialized) return true;

            //save values for undo
            camera = GameObject.Find(cameraName) as GameObject;
            if (camera == null)
            {
                Debug.LogError(string.Format("could not find camera[{0}] at time d:s[{1}:{2}].  This is really bad.  What did you do to the camera?", 
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                return false;
            }

            cameraBody = camera.GetComponentInChildren<CameraBody>();
            if(cameraBody == null)
            {
                Debug.LogError(string.Format("could not find cameraBody component as child of camera[{0}] at time d:s[{1}:{2}].  Why isn't your camera a cinema suites camera?", 
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                return false;
            }

            savePreviousCameraState();

            //ground parameters
            tempCameraPosition = new Vector3Nullable(null, null, null); //if y not specified in our new params, we will propagate last height forward
            tempCameraOrientation = new Vector3Nullable(null, null, null); 

            //find our anchor if specified
            Vector2 anchorPosition;
            if (calculateAnchor(anchor, out anchorPosition))
            {
                tempCameraPosition.X = anchorPosition.x;
                tempCameraPosition.Z = anchorPosition.y;
            }

            //set y directly from oshmirto
            tempCameraPosition.Y = height;

            //set lens 
            ushort tempLens;
            if(CameraActionFactory.lenses.TryGetValue(lensName, out tempLens))
            {
                lensIndex = tempLens;
            }

            //set F Stop
            ushort tempFStop;
            if(CameraActionFactory.fStops.TryGetValue(fStopName, out tempFStop))
            {
                fStopIndex = tempFStop;
            }



            //framing 
            if (lensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue) // direction doesn't matter even if it is specified.  we just calculate y rotation
            {
                
            }
            else if(!lensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue)//direction still doesn't matter since we can't move in the x,z plane
            {

            }
            else if (lensIndex.HasValue && //direction matters here.  
                (!tempCameraPosition.X.HasValue || !tempCameraPosition.Z.HasValue))//also assuming we get x,z in a pair.  if only one is provided, it is invalid and will be ignored
            {

            }
            else //we are calculating everything by framing and direction.  this is going to get a little long.
            {

            }

            //angling must go after framing, since x,z might not be set til we frame.
            //this is potentially problematic for framing things where previous shot was from not eyeline.  
            //perhaps we should be setting the angle down to 0 for our calculations then restore it....
            //lots of opportunity for things to get squirrelly here.
            

            //focus has to go after all possible x,y,z settings to get the correct distance to subject
            Vector3 focusPosition;
            if(calculateFocusPosition(focusTarget,out focusPosition))
            {
                newFocusDistance = Vector3.Distance(newCameraPosition, focusPosition);       
            }

            //sort out what wins where and assign to final camera properties
            //start with previous camera properties in case nothing fills them in
            newCameraPosition = previousCameraPosition;
            newCameraOrientation = previousCameraOrientation;

            return true;
        }


        private void savePreviousCameraState()
        {
            previousCameraOrientation = camera.transform.rotation;
            previousCameraPosition = camera.transform.position;
            previousLensIndex = (ushort)cameraBody.IndexOfLens;
            previousFStopIndex = (ushort)cameraBody.IndexOfFStop;
        }


        /// <summary>
        /// function of position and angleSetting.  this needs our framing to find x-z position if it's not specified.
        /// in the simplest case, x,y,z are already set by the author and we simply set the rotation we want.
        /// </summary>
        private void calculateCameraAngle()
        {

        }

        private bool calculateFocusPosition(string focusTarget, out Vector3 focusPosition)
        {
            focusPosition = new Vector3();
            if (!string.IsNullOrEmpty(focusTarget))
            {
                //try to parse target as a coordinate                
                if (focusTarget.TryParseVector3(out focusPosition))
                {
                    Debug.Log("focus @" + focusPosition);
                    return true;
                }

                //try to find the target as an actor
                var target = GameObject.Find(focusTarget);
                if (target == null)
                {
                    Debug.Log("actor name [" + focusTarget + "] not found. cannot change focus");
                    return false;
                }
                //Debug.Log(string.Format("focus target[{0}] @{1} tracking[{2}]", focusTarget, target.transform.position));
            }
            return true;
        }

        private bool calculateAnchor(string anchor, out Vector2 anchorPosition)
        {
            anchorPosition = new Vector2();
            //if there's nothing there, then nothing to ground to
            if (string.IsNullOrEmpty(anchor)) return false;
            Vector2 planarCoords;
            if (anchor.TryParsePlanarCoords(out planarCoords))
            {
                //we can read the anchor string as planar coords
                anchorPosition = planarCoords;
                return true;
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
                    return false;
                }
                Vector3 actorPosition = actorToAnchorOn.transform.position;
                anchorPosition = new Vector2(actorPosition.x, actorPosition.z);
                return true;
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
            cameraBody.IndexOfLens = previousLensIndex;
            cameraBody.IndexOfFStop = previousFStopIndex;
            
        }

        public void Skip()
        {
            //since this action always happens instantaneously we can assume that the 
            //skip will get run anytime it's selected for addition in the 
            //executing queue in el Presidente
            camera.transform.position = newCameraPosition;
            camera.transform.rotation = newCameraOrientation;
            cameraBody.IndexOfLens = lensIndex.Value;
            cameraBody.IndexOfFStop = fStopIndex.Value;
            if (newFocusDistance.HasValue) cameraBody.FocusDistance = newFocusDistance.Value;
                   
        }

    }
}
