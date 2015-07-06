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

                }
            }
        }
    }
}
