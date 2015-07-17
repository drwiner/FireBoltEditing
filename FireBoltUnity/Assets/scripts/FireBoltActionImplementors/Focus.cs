using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class Focus : IFireBoltAction
    {

        private static readonly string FOCUS_LOCATOR_NAME = "focuser";
        float startTick, endTick;

        string cameraName;
        string targetName;
        Transform focusLocation;
        CameraBody camera;
        GameObject target;
        bool tracking;


        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Focus(float startTick, float endTick, string cameraName, string targetName, bool tracking=false) 
        {
            this.startTick = startTick;
            this.endTick = endTick;
            this.cameraName = cameraName;
            this.targetName = targetName;
            this.tracking = tracking;
        }

        public virtual bool Init()
        {
            //get camera
            camera = GameObject.Find(cameraName).GetComponent<CameraBody>() as CameraBody;
            if (camera == null)
            {
                Debug.LogError("actor name [" + cameraName + "] not found. cannot change focus");
                return false;
            }

            focusLocation = findFocusLocator();
            camera.FocusTransform = focusLocation;

            //try to parse target as a coordinate
            Vector3 focusPosition;
            if (targetName.TryParseVector3(out focusPosition))
            {
                focusLocation.position = focusPosition;
                Debug.Log("focus @" + focusPosition);
                return true;
            }

            //try to find the target as an actor
            target = GameObject.Find(targetName);
            if (target == null)
            {
                Debug.LogError("actor name [" + targetName + "] not found. cannot change focus");
                return false;
            }

            focusLocation.position = target.transform.position;
            Debug.Log(string.Format("focus target[{0}] @{1} tracking[{2}]", targetName, focusLocation.position, tracking));
            return true;
        }

        public virtual void Execute()
        {
            if (tracking)
            {
                //update our unlocked transform object with our target's position
                focusLocation.position = target.transform.position;
                camera.FocusTransform = focusLocation; //not sure why we lose track of the focus locator in the camera, but we'll just put it back
            }        
        }

		public virtual void Undo()
		{

		}

        public virtual void Skip()
        {
            //need to find end position of target if we are tracking, though if we skip, it doesn't matter b/c focus will be overwritten...hmmm
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

        private Transform findFocusLocator()
        {
            var g = GameObject.Find(FOCUS_LOCATOR_NAME);
            if (g == null)
            {
                g = new GameObject(FOCUS_LOCATOR_NAME);                          
            }
            return g.transform;               
        }
    }
}
