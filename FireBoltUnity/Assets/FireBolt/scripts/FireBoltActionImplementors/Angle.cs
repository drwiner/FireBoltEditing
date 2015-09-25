using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CM = CinematicModel;
using LN.Utilities.Collections;
using LN.Utilities;

namespace Assets.scripts
{
    [Obsolete("functionality being folded into ShotFragmentInit",false)]
    class Angle : IFireBoltAction
    {
        // The start and end tick.
        float startTick, endTick;

        // The name of the camera being operated on.
        private String cameraName;

        // The camera body script attached to the camera.
        private GameObject camera;

        // The camera angling setting.
        private Oshmirto.AngleSetting angleSetting;

        // The camera position before the position change.
        private Vector3 oldCameraPosition;

        // The camera rotation before the rotation change.
        private Quaternion oldCameraRotation;

        // The camera position computed by the angling algorithm.
        private Vector3 newCameraPosition;

        // The camera rotation computed by the angling algorithm.
        private Quaternion newCameraRotation;

        // The position of the subject being filmed.
        private Vector3 targetPosition;

        // The name of the subject being filmed.
        private String targetName;

        /// <summary>
        /// Constructs an Angle object given a camera name, an (x,z) anchor, the name of a target GameObject, and a high, medium, or low film angle.
        /// </summary>
        public Angle (float startTick, float endTick, String cameraName, Vector2 anchor, String targetName, Oshmirto.AngleSetting angleSetting)
        {
            // Set the start tick.
            this.startTick = startTick;

            // Set the end tick.
            this.endTick = endTick;

            // Set the type of angle.
            this.angleSetting = angleSetting;

            // Set the name of the camera to use.
            this.cameraName = cameraName;

            // Set the x and z coordinates of the new camera position.
            newCameraPosition = new Vector3(anchor.x, 0, anchor.y);

            // Set the name of the target of the shot.
            this.targetName = targetName;
        }

        public bool Init()
        {
            // Look up the camera game object given its name.
            camera = GameObject.Find(cameraName);

            // Check if we found the camera in the scene.
            if (camera == null)
            {
                // If not, log a debug message that explains the situation.
                Debug.LogError("Camera name [" + cameraName + "] not found. Cannot create " + angleSetting + " camera angle.");

                // Report than an error was encountered.
                return false;
            }

            // Set the camera's initial position.
            oldCameraPosition = camera.transform.position;

            // Set the camera's initial rotation.
            oldCameraRotation = camera.transform.rotation;

            // Look up the target game object given its name.
            GameObject target = GameObject.Find(targetName);

            // Check if the target was found in the scene.
            if (target == null)
            {
                // If not, log a debug message that explains the situation.
                Debug.LogError("Target name [" + targetName + "] not found. Cannot create " + angleSetting + " camera angle.");

                // Report that an error was encountered.
                return false;
            }

            // Set the target's position.
            targetPosition = target.transform.position;

            // Solve the y-coordinate problem and set the camera's new position.
            newCameraPosition = new Vector3(newCameraPosition.x, solveForY(30), newCameraPosition.z);

            // Find the camera's rotation.
            newCameraRotation = Quaternion.LookRotation(targetPosition - newCameraPosition);

            // Report that no errors were encountered.
            return true;
        }

        /// <summary>
        /// Given a shot angle, finds the distance to travel from the target's baseline y position.
        /// Finds the distance by solving the equation: tan(base/hyp angle) * base = height.
        /// Returns the height found by solving the equation.
        /// </summary>
        private float solveForY (float alpha)
        {
            // If the shot is a medium angle it is on the same y-plane as the target.
            if (angleSetting == Oshmirto.AngleSetting.Medium) return targetPosition.y;

            // Otherwise, find the length of the triangle's base by finding the (x,z) distance between the camera and target.
            float baseLength = Mathf.Abs(targetPosition.x - newCameraPosition.x) + Mathf.Abs(targetPosition.z - newCameraPosition.z);

            // Next, find the tangent of the shot angle converted to radians.
            float tanAlpha = Mathf.Tan(Mathf.Deg2Rad * alpha);

            // If this is a high shot move in the positive y direction.
            if (angleSetting == Oshmirto.AngleSetting.High) return baseLength * tanAlpha;

            // Otherwise, move in the negative y direction.
            return baseLength * tanAlpha * -1;
        }

        public void Execute()
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }

        public float StartTick()
        {
            return startTick;
        }

        public float EndTick()
        {
            return endTick;
        }

        public void Undo()
        {
            // Reset the camera's position.
            camera.transform.position = oldCameraPosition;

            // Reset the camera's rotation.
            camera.transform.rotation = oldCameraRotation;
        }

        public void Skip()
        {
            // Set the camera's position.
            camera.transform.position = newCameraPosition;

            // Set the camera's rotation.
            camera.transform.rotation = newCameraRotation;
        }
    }
}
