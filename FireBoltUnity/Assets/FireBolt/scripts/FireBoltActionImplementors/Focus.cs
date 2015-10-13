using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM=CinematicModel;
using LN.Utilities.Collections;

namespace Assets.scripts
{
    public class Focus : FireBoltAction
    {
        string cameraName;
        string targetName;
        Transform focusLocation;
        CameraBody camera;
        GameObject target;
        bool tracking, executed = false;

        public static bool ValidForConstruction(string actorName)
        {
            if (string.IsNullOrEmpty(actorName))
                return false;
            return true;
        }

        public Focus(float startTick, float endTick, string cameraName, string targetName, bool tracking=false) :
            base(startTick, endTick)
        {
            this.cameraName = cameraName;
            this.targetName = targetName;
            this.tracking = tracking;
        }

        public override bool Init()
        {
            //get camera
            camera = GameObject.Find(cameraName).GetComponent<CameraBody>() as CameraBody;
            if (camera == null)
            {
                Debug.LogError("actor name [" + cameraName + "] not found. cannot change focus");
                return false;
            }

            //try to parse target as a coordinate
            Vector3 focusPosition;
            if (targetName.TryParseVector3(out focusPosition))
            {
                Debug.Log("focus @" + focusPosition);
                return true;
            }

            //try to find the target as an actor
            target = GameObject.Find(targetName);
            if (target == null)
            {
                Debug.Log("actor name [" + targetName + "] not found. cannot change focus");
                return false;
            }
            Debug.Log(string.Format("focus target[{0}] @{1} tracking[{2}]", targetName, target.transform.position, tracking));
            return true;
        }

        public override void Execute()
        {
            if (target == null)
            {
                target = GameObject.Find(targetName);
            }

            if (tracking || !executed)
            {
                Vector3 focusPosition;
                if (target != null)
                {
                    camera.FocusDistance = Vector3.Distance(camera.NodalCamera.transform.position, target.transform.position);
                }
                else if(targetName.TryParseVector3(out focusPosition))
                {
                    camera.FocusDistance = Vector3.Distance(camera.NodalCamera.transform.position, focusPosition);                    
                }                
                executed = true;
            }        
        }

		public override void Undo()
		{
            executed = false;
		}

        public override void Skip()
        {

        }

        public override void Stop()
        {
            //nothing to stop
        }

    }
}
