using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Impulse.v_1_336;
using UintT = Impulse.v_1_336.Interval<Impulse.v_1_336.Constants.ValueConstant<uint>, uint>;
using UintV = Impulse.v_1_336.Constants.ValueConstant<uint>;
using Oshmirto;

namespace Assets.scripts
{
    public class CameraActionFactory
    {
        //private static CameraPlan cameraPlan;
        private static FireBoltActionList cameraActionQueue;
        public static FireBoltActionList CreateCameraActions(AStory<UintV, UintT, IIntervalSet<UintV, UintT>> story, string cameraPlanPath)
        {
            cameraActionQueue = new FireBoltActionList();
            CameraPlan cameraPlan = Parser.Parse(cameraPlanPath);
            enqueueCameraActions(cameraPlan, cameraActionQueue);
            return cameraActionQueue;
        }

        private static void enqueueCameraActions(CameraPlan cameraPlan, FireBoltActionList cameraActionQueue)
        {
            Vector3 previousPosition = GameObject.Find("Main Camera").transform.position;
            foreach (Block block in cameraPlan.Blocks)
            {
                foreach (var fragment in block.ShotFragments)
                {
                    Vector3 futurePosition;
                    if (fragment.Anchor.TryParsePlanarCoords(out futurePosition))
                    {
                        bool xLock = false;
                        bool yLock = true;
                        bool zLock = false;
                        cameraActionQueue.Add(new Translate(fragment.StartTime, fragment.StartTime,
                                                            "Main Camera", previousPosition, futurePosition, xLock, yLock, zLock));
                        previousPosition = new Vector3(xLock ? previousPosition.x : futurePosition.x,
                                                       yLock ? previousPosition.y : futurePosition.y,
                                                       zLock ? previousPosition.z : futurePosition.z);
                    }
                    else
                    {
                        //TODO handle camera position calculation
                    }


                    foreach (var movement in fragment.CameraMovements)
                    {
                        switch (movement.Type)
                        {
                            case CameraMovementType.Dolly :
                                switch (movement.Directive)
                                {
                                    case(CameraMovementDirective.With):
                                        cameraActionQueue.Add(new TranslateRelative(movement.Subject, fragment.StartTime, fragment.EndTime, "Main Camera", false, true, false));
                                        break;
                                    case(CameraMovementDirective.To):
                                        Vector3 destination;
                                        if (movement.Subject.TryParsePlanarCoords(out destination))
                                        {
                                            cameraActionQueue.Add(new Translate(fragment.StartTime, fragment.EndTime, "Main Camera",
                                                                                previousPosition, destination, false, true, false));
                                        }
                                        break;
                                }                               
                                break;
                            case CameraMovementType.Crane :
                                switch (movement.Directive)
                                {
                                    case CameraMovementDirective.With:
                                        break;
                                    case CameraMovementDirective.To:
                                        cameraActionQueue.Add(new Translate(fragment.StartTime,fragment.EndTime,"Main Camera",
                                                                            previousPosition, new Vector3(0,float.Parse(movement.Subject),0),true,false,true));
                                        break;
                                }
                                break;
                        }
                    }


                    float rotation = 0f;
                    if (fragment.Framings.Count > 0 && float.TryParse(fragment.Framings[0].FramingTarget, out rotation))
                    {
                        cameraActionQueue.Add(new Rotate(fragment.StartTime, fragment.StartTime, "Main Camera", rotation));
                    }
                    else
                    {
                        //TODO handle calculating actor target facing
                    }



                    // Lens change
                       int lens = lensMMtoIndex(fragment.lensNum);
                        if (lens < 0)
                            Debug.Log("Lens does not exist");
                        else
                            cameraActionQueue.Add(new LensChange(fragment.StartTime, fragment.StartTime, "Main Camera", lens));
                      
                    // FStop change
                        int fstop = fStopToIndex(fragment.fstopType);
                        if (lens < 0)
                            Debug.Log("Lens does not exist");
                        else
                            cameraActionQueue.Add(new FStop(fragment.StartTime, fragment.EndTime, "Main Camera", fstop));


                    // Focus Change
                         cameraActionQueue.Add(new Focus(fragment.StartTime, fragment.EndTime, "Main Camera", fragment.focusPosition));   
                       
                    // Shake it off
                         cameraActionQueue.Add(new Shake(fragment.StartTime, fragment.EndTime, "Main Camera", fragment.shakeValue));   
                }
            }
        }

        private static int lensMMtoIndex(int lensNum)
        {
            if (lensNum == 12) return 0;
            if (lensNum == 14) return 1;
            if (lensNum == 16) return 2;
            if (lensNum == 18) return 3;
            if (lensNum == 21) return 4;
            if (lensNum == 25) return 5;
            if (lensNum == 27) return 6;
            if (lensNum == 32) return 7;
            if (lensNum == 35) return 8;
            if (lensNum == 40) return 9;
            if (lensNum == 50) return 10;
            if (lensNum == 65) return 11;
            if (lensNum == 75) return 12;
            if (lensNum == 100) return 13;
            if (lensNum == 135) return 14;
            if (lensNum == 150) return 15;
            if (lensNum == 180) return 16;
            else
            {
                return -1;
            }
        }

        private static int fStopToIndex(float fstop)
        {
            if (fstop == 1.4) return 0;
            if (fstop == 2) return 1;
            if (fstop == 2.8) return 2;
            if (fstop == 4) return 3;
            if (fstop == 5.6) return 4;
            if (fstop == 8) return 5;
            if (fstop == 11) return 6;
            if (fstop == 16) return 7;
            if (fstop == 22) return 8;
     
            else
            {
                return -1;
            }
        }

    }
}
