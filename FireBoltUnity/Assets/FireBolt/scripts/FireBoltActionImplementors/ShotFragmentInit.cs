using LN.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Oshmirto;

namespace Assets.scripts
{
    public class ShotFragmentInit : IFireBoltAction
    {
        //passed in params
        private float startTick,endTick;
        private bool initialized = false;
        private string anchor=string.Empty;
        private float? height;//TODO implement height specification in oshmirto
        private string cameraName; //this is actually going to manipulate the rig most likely, but what we call it doesn't matter much from in here
        private CameraBody cameraBody; //need a reference to this guy for setting fstop and lens
        private string lensName;
        private string fStopName;
        private List<Framing> framings;
        private Oshmirto.Angle cameraAngle;
        private string focusTarget;

        //parameter grounding
        Vector3Nullable tempCameraPosition;
        Vector3Nullable tempCameraOrientation;
        ushort? tempLensIndex;
        ushort? tempFStopIndex;
        float? tempFocusDistance;

        //saved camera values
        GameObject camera;
        Quaternion previousCameraOrientation = Quaternion.identity;
        Vector3 previousCameraPosition = Vector3.zero;
        ushort previousLensIndex;
        ushort previousFStopIndex;
        float previousFocusDistance;

        //final camera values
        Quaternion newCameraOrientation;
        Vector3 newCameraPosition;
        ushort newLensIndex;
        ushort newFStopIndex;
        float newfocusDistance;

        public ShotFragmentInit(float startTick, float endTick, string cameraName, string anchor, float? height, 
                                string lensName, string fStopName, List<Framing> framings, Oshmirto.Angle cameraAngle, string focusTarget)
        {
            this.startTick = startTick;
            this.endTick = endTick;//used in querying for direction over the shot.  not in setting end of this init action
            this.cameraName = cameraName;
            this.anchor = anchor;
            this.height = height;
            this.lensName = lensName;
            this.fStopName = fStopName;
            this.framings = framings;
            this.cameraAngle = cameraAngle;
            this.focusTarget = focusTarget;
        }

        public bool Init()
        {
            if(initialized) return true;

            if(!findCamera()) return false;           
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
            if(!string.IsNullOrEmpty(lensName) && 
               CameraActionFactory.lenses.TryGetValue(lensName, out tempLens))
            {
                tempLensIndex = tempLens;
            }

            //set F Stop
            ushort tempFStop;
            if(!string.IsNullOrEmpty(fStopName) &&
               CameraActionFactory.fStops.TryGetValue(fStopName, out tempFStop))
            {
                tempFStopIndex = tempFStop;
            }

            //framing 
            GameObject framingTarget; 
            if (framings != null && framings.Count > 0)
            {
                framingTarget = GameObject.Find(framings[0].FramingTarget) as GameObject;
                if (framingTarget != null)
                {
                    //calculate a bounding box for target
                    Bounds targetBounds; 
                    var targetRenderer = framingTarget.GetComponent<Renderer>();
                    
                    if(targetRenderer!=null)
                    {
                        targetBounds = targetRenderer.bounds;
                    }
                    //if the model does not directly have a renderer, accumulate from child bounds
                    else
                    {
                        targetBounds = new Bounds(framingTarget.transform.position, Vector3.zero);
                        foreach (var renderer in framingTarget.GetComponentsInChildren<Renderer>())
                        {
                            targetBounds.Encapsulate(renderer.bounds);
                        }
                        //add some debugging box for the area we think we are framing
                        Vector3 center = targetBounds.center;
                        Vector3 extents = targetBounds.extents;
                        Vector3 [] corners = new Vector3[8];
                        //top face
                        corners[0] = new Vector3(center.x + extents.x, center.y + extents.y, center.z + extents.z);
                        corners[1] = new Vector3(center.x - extents.x, center.y + extents.y, center.z + extents.z);
                        corners[2] = new Vector3(center.x + extents.x, center.y + extents.y, center.z - extents.z);
                        corners[3] = new Vector3(center.x - extents.x, center.y + extents.y, center.z - extents.z);
                        //bottom face
                        corners[4] = new Vector3(center.x + extents.x, center.y - extents.y, center.z + extents.z);
                        corners[5] = new Vector3(center.x - extents.x, center.y - extents.y, center.z + extents.z);
                        corners[6] = new Vector3(center.x + extents.x, center.y - extents.y, center.z - extents.z);
                        corners[7] = new Vector3(center.x - extents.x, center.y - extents.y, center.z - extents.z);
                        //vertical lines
                        for (int i = 0; i < 8; i++)
                        {
                            for (int j = 0; j < 8; j++)
                            {
                                if (i == j) continue;
                                Debug.DrawLine(corners[i], corners[j], Color.cyan, 150);
                                //shows that we are finding a volume for the actor before he is positioned correctly.  
                                //we so we frame for pudge lying down :(
                            }
                        }
                    }

                    Camera nodalCam = Camera.FindObjectOfType<Camera>();
                                        
                    if (tempLensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue) 
                    {
                        //case is here for completeness.  rotation needs to be done for all combinations of lens and anchor specification, so it goes after all the conditionals
                    }
                    else if (!tempLensIndex.HasValue && tempCameraPosition.X.HasValue && tempCameraPosition.Z.HasValue)//direction still doesn't matter since we can't move in the x,z plane
                    {
                        //naively guessing and checking
                        Quaternion savedCameraRotation = nodalCam.transform.rotation;
                        //point the camera at the thing
                        nodalCam.transform.rotation = Quaternion.LookRotation(targetBounds.center - nodalCam.transform.position);
                        float targetFov = 0;
                        while (targetFov < float.Epsilon)
                        {                            
                            //find where on the screen the extents are.  using viewport space so this will be in %. z is world units away from camera
                            Vector3 bMax = nodalCam.WorldToViewportPoint(targetBounds.max);
                            Vector3 bMin = nodalCam.WorldToViewportPoint(targetBounds.min);

                            float FullMinPercent = 0.95f;
                            float FullMaxPercent = 1.0f;
                            float FovStepSize = 0.5f;
                            //compare with static percentages for different framings
                            switch (framings[0].FramingType)
                            {
                                case FramingType.Full:
                                    if (bMax.y - bMin.y > FullMinPercent && bMax.y - bMin.y < FullMaxPercent)
                                    {
                                        targetFov = nodalCam.fieldOfView;
                                        
                                    }
                                    else if (bMax.y - bMin.y > FullMaxPercent)
                                    {
                                        nodalCam.fieldOfView += FovStepSize;//consider making step size a function of current size to increase granularity at low fov
                                    }
                                    else if (bMax.y - bMin.y < FullMinPercent)
                                    {
                                        nodalCam.fieldOfView -= FovStepSize;
                                    }
                                    break;
                                default:
                                    break;
                            }
                            //force matrix recalculations on the camera after adjusting fov
                            nodalCam.ResetProjectionMatrix();
                            nodalCam.ResetWorldToCameraMatrix();                            
                            //raycast to ensure visibility.  don't have to if we were handed the camera position
                            //if failed then try somewhere else                            
                        }
                        //reset camera position...we should only be moving the rig
                        nodalCam.transform.rotation = savedCameraRotation;
                        tempLensIndex = (ushort)ElPresidente.Instance.GetLensIndex(targetFov);
                    }
                    else if (tempLensIndex.HasValue && //direction matters here.  
                        (!tempCameraPosition.X.HasValue || !tempCameraPosition.Z.HasValue))//also assuming we get x,z in a pair.  if only one is provided, it is invalid and will be ignored
                    {

                    }
                    else //we are calculating everything by framing and direction.  this is going to get a little long.
                    {

                    }

                    tempCameraOrientation.Y = Quaternion.LookRotation(framingTarget.transform.position - tempCameraPosition.Merge(previousCameraPosition)).eulerAngles.y;
                }
                else
                {
                    Debug.LogError(string.Format("could not find actor [{0}] at time d:s[{1}:{2}].  Where's your dude?",
                    framings[0].FramingTarget, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                }
            }
            
            //angling must go after framing, since x,z might not be set til we frame.
            //this is potentially problematic for framing things where previous shot was from not eyeline.  
            //perhaps we should be setting the angle down to 0 for our calculations then restore it....
            //lots of opportunity for things to get squirrelly here.
            if (cameraAngle != null && !string.IsNullOrEmpty(cameraAngle.Target))
            {
                // Look up the target game object given its name.
                GameObject angleTarget = GameObject.Find(cameraAngle.Target);

                // Check if the target was found in the scene.
                if (angleTarget != null)
                {
                    if (!tempCameraPosition.Y.HasValue)//only allow angle to adjust height if it is not set manually
                    {
                        tempCameraPosition.Y = solveForYPosition(30f, tempCameraPosition.Merge(previousCameraPosition), angleTarget.transform.position, cameraAngle.AngleSetting);
                    }
                    //choosing only to update x axis rotation if angle is specified.  this means that some fragments where the camera was previously tilted
                    //may fail to show the actor if the fragment only specifies a framing.  we could make angle mandatory...
                    //this is not ideal, but neither is lacking the ability to leave the camera x axis rotation unchanged.
                    //like where we do a tilt with and then lock off
                    tempCameraOrientation.X = Quaternion.LookRotation(angleTarget.transform.position - tempCameraPosition.Merge(previousCameraPosition)).eulerAngles.x;
                }
                else
                {
                    Debug.LogError(string.Format("could not find actor [{0}] at time d:s[{1}:{2}].  Where's your dude?",
                    cameraAngle.Target, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                }                
            }

            //focus has to go after all possible x,y,z settings to get the correct distance to subject
            Vector3 focusPosition;
            if(calculateFocusPosition(focusTarget,out focusPosition))
            {
                tempFocusDistance = Vector3.Distance(tempCameraPosition.Merge(previousCameraPosition), focusPosition);       
            }

            //sort out what wins where and assign to final camera properties
            //start with previous camera properties in case nothing fills them in
            newCameraPosition = tempCameraPosition.Merge(previousCameraPosition);
            newCameraOrientation = Quaternion.Euler(tempCameraOrientation.Merge(previousCameraOrientation.eulerAngles));
            newLensIndex = tempLensIndex.HasValue ? tempLensIndex.Value : previousLensIndex;
            newFStopIndex = tempFStopIndex.HasValue ? tempFStopIndex.Value : previousFStopIndex;
            newfocusDistance = tempFocusDistance.HasValue ? tempFocusDistance.Value : previousFocusDistance;

            return true;
        }


        /// <summary>
        /// Given a shot angle, finds the distance to travel from the target's baseline y position.
        /// Finds the distance by solving the equation: tan(base/hyp angle) * base = height.
        /// Returns the height found by solving the equation.
        /// </summary>
        private float solveForYPosition(float alpha, Vector3 sourcePosition, Vector3 targetPosition, AngleSetting angleSetting)
        {
            // If the shot is a medium angle it is on the same y-plane as the target.
            if (angleSetting == Oshmirto.AngleSetting.Medium) return targetPosition.y;

            // Otherwise, find the length of the triangle's base by finding the (x,z) distance between the camera and target.
            float baseLength = Mathf.Abs(targetPosition.x - sourcePosition.x) + Mathf.Abs(targetPosition.z - sourcePosition.z);

            // Next, find the tangent of the shot angle converted to radians.
            float tanAlpha = Mathf.Tan(Mathf.Deg2Rad * alpha);

            // If this is a high shot move in the positive y direction.
            if (angleSetting == Oshmirto.AngleSetting.High) return baseLength * tanAlpha;

            // Otherwise, move in the negative y direction.
            return baseLength * tanAlpha * -1 + targetPosition.y;
        }

        /// <summary>
        /// capturing state for Undo()'ing
        /// </summary>
        private void savePreviousCameraState()
        {
            previousCameraOrientation = camera.transform.rotation;
            previousCameraPosition = camera.transform.position;
            previousLensIndex = (ushort)cameraBody.IndexOfLens;
            previousFStopIndex = (ushort)cameraBody.IndexOfFStop;
            previousFocusDistance = cameraBody.FocusDistance;
        }

        private bool findCamera()
        {
            camera = GameObject.Find(cameraName) as GameObject;
            if (camera == null)
            {
                Debug.LogError(string.Format("could not find camera[{0}] at time d:s[{1}:{2}].  This is really bad.  What did you do to the camera?",
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                return false;
            }

            cameraBody = camera.GetComponentInChildren<CameraBody>();
            if (cameraBody == null)
            {
                Debug.LogError(string.Format("could not find cameraBody component as child of camera[{0}] at time d:s[{1}:{2}].  Why isn't your camera a cinema suites camera?",
                    cameraName, ElPresidente.Instance.CurrentDiscourseTime, ElPresidente.Instance.CurrentDiscourseTime));
                return false;
            }
            return true;
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
                focusPosition = target.transform.position;
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
            cameraBody.FocusDistance = previousFocusDistance;
            
        }

        public void Skip()
        {
            //since this action always happens instantaneously we can assume that the 
            //skip will get run anytime it's selected for addition in the 
            //executing queue in el Presidente
            camera.transform.position = newCameraPosition;
            camera.transform.rotation = newCameraOrientation;
            cameraBody.IndexOfLens = newLensIndex;
            cameraBody.IndexOfFStop = newFStopIndex;
            cameraBody.FocusDistance = newfocusDistance;                  
        }

    }
}
